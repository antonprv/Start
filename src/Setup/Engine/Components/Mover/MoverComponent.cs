// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Engine.Components.Camera;
using Engine.Components.Mover.Resources;
using Engine.Services.Input;
using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Interfaces;
using Framework.Components.Mover.Debug;
using Framework.Components.Mover.Presets;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Framework.Logger;

using Godot;
using Physics;
using System.Collections.Generic;
using Zenjex;

using CollisionLayer = Physics.CollisionLayer;

namespace Engine.Components.Mover
{
	public partial class MoverComponent : BepuCharacterBody3D, IMoverComponent
	{
		[ExportGroup( "Rotation" )]
		[Export] private float _rotationSpeed = 6f;

		[ExportGroup( "Data Objects" )]
		[Export] public MProfile Profile { get; set; }
		[Export] public MovementMode InitialMode { get; set; } = MovementMode.Quake;

		[ExportGroup( "References" )]
		[Export] private MeshInstance3D _playerMesh;
		[Export] private CameraComponent _cameraComponent;

		[ExportGroup( "Debug" )]
		[Export] private bool _showDebug = false;

		[ExportGroup( "Noclip" )]
		[Export] private float _noclipSpeed = 10f;

		#region Private state

		private IMovementMotor _motor;
		private MovementContext _context;
		private MovementDebugOverlay _debugOverlay;
		private MovementMode _currentMode;

		#endregion

		#region Noclip

		private bool _noclip;

		// Collision masks saved before noclip so we can restore them on toggle-off.
		private CollisionLayer _savedCollisionLayer;
		private uint _savedCollisionMask;

		/// <summary>
		/// Toggle noclip mode.
		/// When active: collisions are disabled, jump is suppressed, and the player
		/// flies freely in the direction the camera is facing (including vertical).
		/// Equivalent to Doom's IDCLIP cheat.
		/// </summary>
		public void SetNoclip( bool enabled )
		{
			if ( _noclip == enabled )
				return;

			_noclip = enabled;

			_cameraComponent.SetNoclip( _noclip );

			if ( _noclip )
			{
				_savedCollisionLayer = Layer;
				Layer = CollisionLayer.None;

				// Kill any existing vertical momentum so the player doesn't float away.
				Velocity.Value = new Vector3( Velocity.Value.X, 0f, Velocity.Value.Z );
			}
			else
			{
				Layer = _savedCollisionLayer;

				// Zero out velocity so the player doesn't shoot off after landing.
				Velocity.Value = Vector3.Zero;
			}

			GameLogger.LogInfo( $"Got noclip: {( _noclip ? "ON" : "OFF" )}" );
		}

		public bool IsNoclip => _noclip;

		#endregion

		#region Services

		private IInputService _inputService;
		private Vector3 _inputDirection;
		private bool _jumpInput;

		[Inject]
		private void Construct( IInputService inputService ) => _inputService = inputService;

		#endregion

		#region Godot lifecycle

		protected override void OnRegister()
		{
			base.OnRegister();

			if ( Profile == null )
				Profile = QuakePreset.DefaultProfile().Convert();

			_currentMode = InitialMode;
			_motor = new MovementMotor( BuildTraitsForMode( _currentMode ) );

			if ( _showDebug )
			{
				_debugOverlay = new MovementDebugOverlay();
				AddChild( _debugOverlay );
			}
		}

		public override void _PhysicsProcess( double delta )
		{
			HandleInput();

			if ( _noclip )
				SimulateNoclip( delta );
			else
				SimulateMovement( delta );

			DisplayRotation( delta );

			MoveAndSlide( delta );

			// MoveAndSlide() may have clipped Velocity against a wall/corner this tick (see
			// BepuCharacterBody3D). The motor owns its own persistent velocity for the accel/
			// friction traits, so that correction has to be mirrored back into it - otherwise
			// the motor keeps accelerating a stale, uncorrected vector and a corner hit stops
			// the character's position without ever stopping its stored velocity.
			if ( !_noclip )
				_motor.Velocity = Velocity.Value;

			ShowDebugOverlay();
		}

		private void HandleInput()
		{
			GetInputDirection();
			GetJumpInput();
		}

		protected virtual void GetInputDirection()
		{
			Vector2 input = _inputService.GetInputVector();

			// Noclip: fly along the full camera vector (including pitch).
			if ( _noclip )
			{
				if ( _cameraComponent != null )
				{
					Vector3 camForward = _cameraComponent.GetForwardDirection();
					Vector3 camRight = _cameraComponent.GetRightDirection();

					if ( !camForward.IsNearlyZero() ) camForward.FastNormalize();
					if ( !camRight.IsNearlyZero() ) camRight.FastNormalize();

					_inputDirection = ( camRight * input.X ) + ( camForward * -input.Y );
				}
				else
				{
					// Fallback: no camera, fly in world XZ only.
					_inputDirection = new Vector3( input.X, 0, -input.Y );
				}

				return;
			}

			// Normal: project onto horizontal plane.
			if ( _cameraComponent == null )
			{
				_inputDirection = new Vector3( input.X, 0, -input.Y );
				return;
			}

			Vector3 cf = _cameraComponent.GetForwardDirection();
			cf.Y = 0;
			if ( !cf.IsNearlyZero() )
				cf.FastNormalize();

			Vector3 cr = _cameraComponent.GetRightDirection();
			cr.Y = 0;
			if ( !cr.IsNearlyZero() )
				cr.FastNormalize();

			_inputDirection = ( cr * input.X ) + ( cf * -input.Y );
			_inputDirection.Y = 0;
		}

		// Jump input is read but never applied in noclip — kept for consistency.
		protected virtual void GetJumpInput() => _jumpInput = _inputService.IsJumpPressed();

		private void SimulateNoclip( double delta )
		{
			// Direct velocity from input; no gravity, no jump, no traits.
			Velocity.Value = _inputDirection.IsNearlyZero()
				? Vector3.Zero
				: _inputDirection.FastNormalized() * _noclipSpeed;
		}

		private void SimulateMovement( double delta )
		{
			float dt = (float)delta;

			// Build context
			_context.WishDirection = _inputDirection;
			_context.JumpRequested = _jumpInput;
			_context.IsOnFloor = IsOnFloor.Value;
			_context.Gravity = Gravity.Value;
			_context.Delta = dt;   // motor also sets this, but set here for clarity
			_context.Profile = Profile;
			_context.JumpConsumed = false; // reset each tick; JumpTrait sets it when jump fires

			// Simulate
			_motor.Simulate( dt, _context );

			// Apply to CharacterBody3D
			Velocity.Value = _motor.Velocity;
		}

		private void DisplayRotation( double delta )
		{
			if ( _playerMesh == null || _inputDirection.IsNearlyZero() )
				return;

			// Convert world-space direction to local space of this CharacterBody3D
			// so the mesh orients correctly regardless of root node rotation.
			Vector3 localInput = GlobalTransform.Basis.Inverse() * _inputDirection;

			float targetAngle = FMath.FastAtan2( -localInput.X, -localInput.Z );

			Quaternion targetRotation = FMath.FromAxisAngle( Vector3.Up, targetAngle );
			Quaternion currentRotation = _playerMesh.Quaternion;
			Quaternion smoothRotation = currentRotation.FastSlerp( targetRotation, (float)( _rotationSpeed * delta ) );

			_playerMesh.Quaternion = smoothRotation;
		}

		private void ShowDebugOverlay() => _debugOverlay?.UpdateOverlay(
			GlobalPosition + Vector3.Up * 0.1f,
			Velocity.Value,
			_context.WishDirection,
			IsOnFloor.Value,
			_currentMode
		);

		#endregion


		#region Runtime mode switching

		/// <summary>
		/// Hot-swap movement behavior at runtime.
		/// Current velocity is fully preserved - only the trait list changes.
		/// Equivalent to UE's SetMovementMode(EMovementMode).
		/// </summary>
		public void SetMovementMode( MovementMode mode )
		{
			if ( _currentMode == mode )
				return;

			_currentMode = mode;
			_motor.SetTraits( BuildTraitsForMode( mode ) );

			GameLogger.LogInfo( $"Mode -> {mode}" );
		}

		/// <summary>Current active movement mode.</summary>
		public MovementMode CurrentMode => _currentMode;

		#endregion

		#region Preset Wiring

		private static List<IMovementTrait> BuildTraitsForMode( MovementMode mode ) =>
			mode switch
			{
				MovementMode.Quake => QuakePreset.Build(),
				MovementMode.Realistic => RealisticPreset.Build(),
				MovementMode.Hybrid => HybridPreset.Build(),
				MovementMode.Doom3 => Doom3Preset.Build(),
				_ => QuakePreset.Build()
			};

		#endregion
	}
}