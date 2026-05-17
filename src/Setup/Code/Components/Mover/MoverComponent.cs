// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Code.Common.Extensions.Logging;
using Code.Services.Input;

using System.Collections.Generic;

using Components.Mover.Core;
using Components.Mover.Presets;
using Components.Mover.Debug;

using FastMath;
using Godot;
using ZenjexGodot;

namespace Code.Components.Mover
{
	public partial class MoverComponent : CharacterBody3D
	{
		[ExportGroup( "Rotation" )]
		[Export] private float _rotationSpeed = 6f;

		[ExportGroup("Data Objects")]
		[Export] public MovementProfile Profile { get; set; }
		[Export] public MovementMode InitialMode { get; set; } = MovementMode.Quake;

		[ExportGroup( "References" )]
		[Export] private MeshInstance3D _playerMesh;

		[ExportGroup("Debug")]
		[Export] private bool _showDebug = false;

		#region Private state

		private IMovementMotor _motor;
		private MovementContext _context;
		private MovementDebugOverlay _debugOverlay;
		private MovementMode _currentMode;

		#endregion

		#region Services

		private IInputService _inputService;
		private IGameLog _logger;
		private Vector3 _inputDirection;
		private bool _jumpInput;

		[Inject]
		private void Construct( 
			IInputService inputService,
			IGameLog logger
		)
		{
			_inputService = inputService;
			_logger = logger;
		}

		public override void _EnterTree() => DiContainer.Instance.Inject( this );

		#endregion

		#region Godot lifecycle

		public override void _Ready()
		{
			if ( Profile == null )
				Profile = QuakePreset.DefaultProfile();

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

			SimulateMovement( delta );
			DisplayRotation( delta );
			
			MoveAndSlide();

			ShowDebugOverlay();
		}

		private void HandleInput()
		{
			GetInputDirection();
			GetJumpInput();
		}

		protected virtual void GetInputDirection() => _inputDirection =
			new Vector3(
				_inputService.GetInputVector().X,
				0,
				_inputService.GetInputVector().Y
			);

		protected virtual void GetJumpInput() => _jumpInput = _inputService.IsJumpPressed();


		private void SimulateMovement( double delta )
		{
			float dt = (float)delta;

			// Build context
			_context.WishDirection = _inputDirection;
			_context.JumpRequested = _jumpInput;
			_context.IsOnFloor = IsOnFloor();
			_context.Gravity = GetGravity();
			_context.Delta = dt;   // motor also sets this, but set here for clarity
			_context.Profile = Profile;
			_context.JumpConsumed = false; // reset each tick; JumpTrait sets it when jump fires

			// Simulate
			_motor.Simulate( dt, _context );

			// Apply to CharacterBody3D
			Velocity = _motor.Velocity;
		}

		private void DisplayRotation( double delta )
		{
			if ( _playerMesh == null || _inputDirection.IsNearlyZero() )
				return;

			float targetAngle = FMath.FastAtan2( -_inputDirection.X, -_inputDirection.Z );

			Quaternion targetRotation = new Quaternion( Vector3.Up, targetAngle );
			Quaternion currentRotation = _playerMesh.Quaternion;
			Quaternion smoothRotation = currentRotation.FastSlerp( targetRotation, (float)( _rotationSpeed * delta ) );

			_playerMesh.Quaternion = smoothRotation;
		}

		private void ShowDebugOverlay() => _debugOverlay?.UpdateOverlay(
			GlobalPosition + Vector3.Up * 0.1f,
			Velocity,
			_context.WishDirection,
			IsOnFloor(),
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

			_logger.LogInfo( $"Mode → {mode}" );
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
				_ => QuakePreset.Build()
			};

		#endregion
	}
}
