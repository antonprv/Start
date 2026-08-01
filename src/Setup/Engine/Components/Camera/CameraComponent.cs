// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Engine.Services.Input;
using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Framework.Components.Camera.Core.Types;
using Framework.Components.Camera.Traits.Pose;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Framework.Logger;
using Godot;
using Physics;
using System.Collections.Generic;
using System.Linq;
using Zenjex;
using CollisionLayer = Physics.CollisionLayer;

namespace Engine.Components.Camera
{
	public partial class CameraComponent : Node3D
	{
		#region Inspector Properties

		[ExportGroup( "Look Input" )]
		[Export] public float MouseSensitivity { get; set; } = 0.1f;

		[ExportGroup( "Field of View" )]
		[Export] public float DefaultFOV { get; set; } = 60f;
		[Export] public float MinFOV { get; set; } = 1f;
		[Export] public float MaxFOV { get; set; } = 170f;

		[ExportGroup( "Edge Scroll" )]
		[Export] public bool EdgeScrollEnabled { get; set; } = true;
		[Export] public float EdgeScrollMarginPx { get; set; } = 12f;

		[ExportGroup( "Orbit" )]
		[Export] public MouseButton OrbitButton { get; set; } = MouseButton.Right;

		[ExportGroup( "Data Objects" )]
		[Export] public CameraPreset InitialMode { get; set; } = CameraPreset.GeneralThirdPerson;

		[ExportGroup( "References" )]
		[Export] private Camera3D _camera;
		[Export] private BepuSpringArm3D _springArm;
		[Export] private Node3D _followTarget;

		#endregion

		#region Injection

		private IInputService _inputService;

		[Inject]
		private void Construct( IInputService inputService ) => _inputService = inputService;

		public override void _EnterTree() => DiContainer.Instance.Inject( this );

		#endregion

		#region Private state

		private ICameraMotor _motor;
		private List<ICameraTrait> _activeTraits;
		private CameraPreset _currentMode;
		private CameraContext _context;

		private bool _hasPreviousTargetPosition;
		private Vector3 _previousTargetPosition;
		private Vector3 _smoothedTargetVelocity;

		private float _accumulatedZoom;
		private bool _noclip;

		private CollisionLayer _savedCollisionLayer;
		private CollisionLayer _savedCollisionMask;

		#endregion

		#region Godot lifecycle

		public override void _Ready()
		{
			InitializeReferences();

			_currentMode = InitialMode;
			_activeTraits = BuildTraitsForMode( _currentMode );
			_motor = new CameraMotor( _activeTraits );

			SeedInitialState();
		}

		public override void _Process( double delta )
		{
			BuildContext( delta );

			_motor.Simulate( (float)delta, _context );

			ApplyRigState( _motor.State );
		}

		public override void _UnhandledInput( InputEvent @event )
		{
			if ( @event is not InputEventMouseButton mb || !mb.Pressed )
				return;

			if ( mb.ButtonIndex == MouseButton.WheelUp )
				_accumulatedZoom += 1f;
			else if ( mb.ButtonIndex == MouseButton.WheelDown )
				_accumulatedZoom -= 1f;
		}

		private void InitializeReferences()
		{
			if ( _camera != null )
			{
				_camera.Fov = DefaultFOV;
				_camera.Current = true;
			}
		}

		private void SeedInitialState()
		{
			var state = _motor.State;

			if ( _springArm != null )
			{
				Vector3 euler = _springArm.RotationDegrees;
				state.TargetYaw = state.Yaw = euler.Y;
				state.TargetPitch = state.Pitch = euler.X;
			}

			state.TargetFOV = state.FOV = DefaultFOV;
			state.PivotPosition = _followTarget?.GlobalPosition ?? GlobalPosition;

			_motor.State = state;
			_previousTargetPosition = state.PivotPosition;
		}

		#endregion

		#region Context building

		private void BuildContext( double delta )
		{
			float dt = (float)delta;

			Vector3 targetPosition = _followTarget?.GlobalPosition ?? GlobalPosition;
			UpdateTargetVelocity( targetPosition, dt );

			_context.TargetPosition = targetPosition;
			_context.TargetVelocity = _smoothedTargetVelocity;
			_context.TargetBasis = _followTarget?.GlobalTransform.Basis ?? Basis.Identity;

			_context.LookInput = _inputService.GetCameraVector() * MouseSensitivity;
			_context.PanInput = ComputeEdgeScrollInput();
			_context.ZoomInput = _accumulatedZoom;
			_accumulatedZoom = 0f;

			_context.OrbitHeld = Input.IsMouseButtonPressed( OrbitButton );
			_context.Delta = dt;
		}

		private void UpdateTargetVelocity( Vector3 targetPosition, float dt )
		{
			if ( !_hasPreviousTargetPosition || dt <= 0f )
			{
				_previousTargetPosition = targetPosition;
				_hasPreviousTargetPosition = true;
				return;
			}

			Vector3 instantVelocity = ( targetPosition - _previousTargetPosition ) / dt;
			_previousTargetPosition = targetPosition;

			// Light low-pass so a single noisy frame doesn't spike CameraLagTrait.
			_smoothedTargetVelocity = _smoothedTargetVelocity.FastLerp(
				instantVelocity, FMath.Clamp01( dt * 10f ) );
		}

		private Vector2 ComputeEdgeScrollInput()
		{
			if ( !EdgeScrollEnabled )
				return Vector2.Zero;

			Window window = GetWindow();
			if ( window == null || !window.HasFocus() )
				return Vector2.Zero;

			Viewport viewport = GetViewport();
			Vector2 mousePos = viewport.GetMousePosition();
			Vector2 size = viewport.GetVisibleRect().Size;

			Vector2 pan = Vector2.Zero;

			if ( mousePos.X <= EdgeScrollMarginPx )
				pan.X = -1f;
			else if ( mousePos.X >= size.X - EdgeScrollMarginPx )
				pan.X = 1f;

			if ( mousePos.Y <= EdgeScrollMarginPx )
				pan.Y = -1f;
			else if ( mousePos.Y >= size.Y - EdgeScrollMarginPx )
				pan.Y = 1f;

			return pan;
		}

		#endregion

		#region Applying rig state to the scene

		private void ApplyRigState( CameraRigState state )
		{
			float pitch = FMath.Clamp( state.Pitch, -89f, 89f );
			float yaw = state.Yaw;

			Vector3 eulerRad = new Vector3( pitch * FMath.Deg2Rad, yaw * FMath.Deg2Rad, 0f );
			Basis rotBasis = Basis.FromEuler( eulerRad, EulerOrder.Yxz );

			Vector3 rotatedOffset = rotBasis * state.LocalOffset;
			GlobalPosition = state.PivotPosition + state.PanOffset + rotatedOffset;

			if ( _springArm != null )
			{
				Vector3 armScale = _springArm.Scale;
				_springArm.Basis = rotBasis;
				_springArm.Scale = armScale;

				_springArm.SpringLength = FMath.Max( 0.05f, state.ArmLength + state.ExtraDistance + state.OvershootDistance );
			}

			if ( _camera != null )
				_camera.Fov = FMath.Clamp( state.FOV, MinFOV, MaxFOV );
		}

		#endregion

		#region Noclip

		public bool IsNoclip => _noclip;

		public void SetNoclip( bool enabled )
		{
			if ( _noclip == enabled )
				return;

			_noclip = enabled;

			if ( _springArm == null )
				return;

			if ( _noclip )
			{
				_savedCollisionLayer = _springArm.Layer;
				_savedCollisionMask = _springArm.Mask;

				_springArm.Layer = CollisionLayer.None;
				_springArm.Mask = CollisionLayer.None;
			}
			else
			{
				_springArm.Layer = _savedCollisionLayer;
				_springArm.Mask = _savedCollisionMask;
			}

			GameLogger.LogInfo( $"Got noclip: {( _noclip ? "ON" : "OFF" )}" );
		}

		#endregion

		#region Public API

		/// <summary>Set the target to follow. Pass null to stop following (rig holds its last position).</summary>
		public void SetFollowTarget( Node3D target )
		{
			_followTarget = target;
			_hasPreviousTargetPosition = false;
		}

		/// <summary>Nudge yaw/pitch immediately (both the smoothed and raw-target values), e.g. for scripted camera kicks.</summary>
		public void AddRotation( float deltaYaw, float deltaPitch )
		{
			var state = _motor.State;

			state.TargetYaw += deltaYaw;
			state.Yaw += deltaYaw;

			state.TargetPitch = FMath.Clamp( state.TargetPitch + deltaPitch, -89f, 89f );
			state.Pitch = FMath.Clamp( state.Pitch + deltaPitch, -89f, 89f );

			_motor.State = state;
		}

		/// <summary>Instantly snap the rig to a world position, bypassing follow/pan/smoothing for one frame.</summary>
		public void SetCameraPosition( Vector3 position )
		{
			var state = _motor.State;
			state.PivotPosition = position;
			state.PanOffset = Vector3.Zero;
			_motor.State = state;

			GlobalPosition = position;
		}

		/// <summary>Set the desired spring-arm length. Still runs through whatever smoothing the active traits use.</summary>
		public void SetSpringArmLength( float length )
		{
			var state = _motor.State;
			state.TargetArmLength = FMath.Max( 0.1f, length );
			_motor.State = state;
		}

		/// <summary>Set the desired field of view (clamped to MinFOV/MaxFOV). Runs through SmoothingTrait's FOV rate.</summary>
		public void SetFOV( float fov )
		{
			var state = _motor.State;
			state.TargetFOV = FMath.Clamp( fov, MinFOV, MaxFOV );
			_motor.State = state;
		}

		/// <summary>
		/// Smoothly blend the rig's shoulder offset (and, optionally, arm length/FOV) to a new
		/// pose over `duration` seconds - e.g. ease the camera up when the character crouches,
		/// or back to the normal over-the-shoulder framing when they stand and walk. No-ops if
		/// the active mode doesn't include a PoseTrait.
		/// </summary>
		public void TransitionToPose( Vector3 localOffset, float armLength = -1f, float fov = -1f, float duration = -1f )
		{
			var poseTrait = _activeTraits?.OfType<PoseTrait>().FirstOrDefault();
			poseTrait?.SetPose( new CameraPose( localOffset, armLength, fov ), duration );
		}

		/// <summary>Current smoothed yaw in degrees (excludes transient overshoot wobble).</summary>
		public float GetYaw() => _motor.State.Yaw;

		/// <summary>Current smoothed pitch in degrees (excludes transient overshoot wobble).</summary>
		public float GetPitch() => _motor.State.Pitch;

		/// <summary>Current camera forward direction.</summary>
		public Vector3 GetForwardDirection() => -_camera.GlobalTransform.Basis.Z;

		/// <summary>Current camera right direction.</summary>
		public Vector3 GetRightDirection() => _camera.GlobalTransform.Basis.X;

		#endregion

		#region Runtime mode switching

		/// <summary>
		/// Hot-swap camera behavior at runtime. Yaw/pitch/arm-length/FOV/offset all carry over
		/// unchanged - only which traits process them changes, so switching e.g. third-person ->
		/// first-person doesn't pop, it just stops being clamped/offset the way it was.
		/// Equivalent to MoverComponent.SetMovementMode.
		/// </summary>
		public void SetCameraMode( CameraPreset mode )
		{
			if ( _currentMode == mode )
				return;

			_currentMode = mode;
			_activeTraits = BuildTraitsForMode( mode );
			_motor.SetTraits( _activeTraits );

			GameLogger.LogInfo( $"Camera mode -> {mode}" );
		}

		/// <summary>Current active camera mode.</summary>
		public CameraPreset CurrentMode => _currentMode;

		#endregion

		#region Preset wiring

		private List<ICameraTrait> BuildTraitsForMode( CameraPreset preset )
		{
			if ( preset == CameraPreset.Custom )
				return GetCustomTraits();

			return CameraPresetFactory.Create( preset ).Build();
		}

		private List<ICameraTrait> GetCustomTraits() =>
			CameraPresetFactory.Create( CameraPreset.GeneralThirdPerson ).Build();

		#endregion
	}
}
