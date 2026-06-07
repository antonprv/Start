// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using FastMath;
using Game.Code.Services.Input;
using Godot;
using Zenjex;

namespace Game.Code.Components.Camera
{
	/// <summary>
	/// Advanced camera component for Godot 4.x
	/// Supports camera lag, rotation constraints, and spring arm configuration
	/// Works with existing hierarchy: CameraRoot (Node3D) -> SpringArm3D -> Camera3D
	/// </summary>
	public partial class CameraComponent : Node3D
	{
		#region Inspector Properties

		[ExportGroup( "Camera Input" )]
		[Export] public float MouseSensitivity = 0.1f;

		[ExportGroup( "Camera Parameters" )]
		[Export] public float PositionLagSpeed = 10f;
		[Export] public float RotationLagSpeed = 15f;
		[Export] public float DefaultFOV = 60f;
		[Export] public float MinPitch = -90f;
		[Export] public float MaxPitch = 90f;
		[Export] public float SpringArmLength = 5f;
		[Export] public float VerticalOffset = 1.6f;

		[ExportGroup( "References" )]
		[Export] private Camera3D _camera;
		[Export] private SpringArm3D _springArm;
		[Export] private Node3D _followTarget;

		#endregion

		#region Injection

		IInputService _inputService;

		[Inject]
		private void Construct( IInputService inputService ) =>
			_inputService = inputService;

		public override void _EnterTree() => DiContainer.Instance.Inject( this );

		#endregion

		#region Private Fields

		private Vector3 _targetPosition;
		private Vector2 _cameraInput;

		private float _targetYaw;
		private float _targetPitch;

		private float _currentYaw;
		private float _currentPitch;

		#endregion

		#region Public API

		public override void _Ready()
		{
			InitializeReferences();
			_targetPosition = GlobalPosition;

			if ( _springArm != null )
			{
				Vector3 euler = _springArm.RotationDegrees;
				_currentYaw = _targetYaw = euler.Y;
				_currentPitch = _targetPitch = euler.X;
			}
		}

		public override void _Process( double delta )
		{
			if ( _followTarget != null )
				UpdateTargetPosition();

			HandleInput();

			UpdateCameraPosition( delta );
			UpdateCameraRotation( delta );
		}

		private void HandleInput()
		{
			Vector2 input = _inputService.GetCameraVector();

			_targetYaw += -input.X * MouseSensitivity;
			_targetPitch = FMath.Clamp( _targetPitch - input.Y * MouseSensitivity, MinPitch, MaxPitch );
		}

		/// <summary>
		/// Set the target to follow. Pass null to stop following.
		/// </summary>
		public void SetFollowTarget( Node3D target ) => _followTarget = target;

		/// <summary>
		/// Add to current rotation (relative input)
		/// </summary>
		public void AddRotation( float deltaYaw, float deltaPitch )
		{
			_currentYaw += deltaYaw;
			_currentPitch = FMath.Clamp( _currentPitch + deltaPitch, MinPitch, MaxPitch );
		}

		/// <summary>
		/// Instantly set camera position
		/// </summary>
		public void SetCameraPosition( Vector3 position )
		{
			_targetPosition = position;
			GlobalPosition = position;
		}

		/// <summary>
		/// Set spring arm length
		/// </summary>
		public void SetSpringArmLength( float length )
		{
			SpringArmLength = FMath.Max( 0.1f, length );
			if ( _springArm != null )
				_springArm.SpringLength = SpringArmLength;
		}

		/// <summary>
		/// Set field of view
		/// </summary>
		public void SetFOV( float fov )
		{
			if ( _camera != null )
				_camera.Fov = FMath.Clamp( fov, 1f, 170f );
		}

		/// <summary>
		/// Get current camera forward direction
		/// </summary>
		public Vector3 GetForwardDirection() => -_camera.GlobalTransform.Basis.Z;

		/// <summary>
		/// Get current camera right direction
		/// </summary>
		public Vector3 GetRightDirection() => _camera.GlobalTransform.Basis.X;

		/// <summary>
		/// Get current yaw rotation in degrees
		/// </summary>
		public float GetYaw() => _currentYaw;

		/// <summary>
		/// Get current pitch rotation in degrees
		/// </summary>
		public float GetPitch() => _currentPitch;

		#endregion

		#region Private Methods

		private void InitializeReferences()
		{
			// Configure spring arm
			if ( _springArm != null )
				_springArm.SpringLength = SpringArmLength;

			// Configure camera
			if ( _camera != null )
			{
				_camera.Fov = DefaultFOV;
				_camera.Current = true;
			}
		}

		private void UpdateTargetPosition()
		{
			if ( _followTarget != null )
			{
				_targetPosition = _followTarget.GlobalPosition;
				_targetPosition.Y += VerticalOffset;
			}
		}

		private void UpdateCameraPosition( double delta )
		{
			if ( PositionLagSpeed <= 0 )
			{
				GlobalPosition = _targetPosition;
				return;
			}

			// Simple exponential smoothing (Lerp)
			float lerpFactor = FMath.Clamp( (float)delta * PositionLagSpeed, 0f, 1f );
			GlobalPosition = GlobalPosition.FastLerp( _targetPosition, lerpFactor );
		}

		private void UpdateCameraRotation( double delta )
		{
			if ( RotationLagSpeed <= 0 )
			{
				_currentYaw = _targetYaw;
				_currentPitch = _targetPitch;
				ApplyRotation();
				return;
			}

			float t = FMath.Clamp( (float)delta * RotationLagSpeed, 0f, 1f );

			_currentYaw = FMath.LerpAngle( _currentYaw, _targetYaw, t );
			_currentPitch = FMath.Lerp( _currentPitch, _targetPitch, t );

			ApplyRotation();
		}

		private void ApplyRotation()
		{
			if ( _springArm == null ) return;

			Vector3 scale = _springArm.Scale;

			Vector3 eulerRad = new Vector3(
				_currentPitch * FMath.Deg2Rad,
				_currentYaw * FMath.Deg2Rad,
				0f
			);
			_springArm.Basis = Basis.FromEuler( eulerRad, EulerOrder.Yxz );

			_springArm.Scale = scale;
		}

		#endregion
	}
}
