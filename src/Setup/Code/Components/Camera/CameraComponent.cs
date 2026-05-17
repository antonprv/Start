using FastMath;
using Godot;

/// <summary>
/// Advanced camera component for Godot 4.x
/// Supports camera lag, rotation constraints, and spring arm configuration
/// Works with existing hierarchy: CameraRoot (Node3D) -> SpringArm3D -> Camera3D
/// </summary>
public partial class CameraComponent : Node3D
{
	#region Inspector Properties

	[ExportGroup("Camera Parameters")]
	[Export] public float PositionLagSpeed = 10f;
	[Export] public float RotationLagSpeed = 15f;
	[Export] public float DefaultFOV = 60f;
	[Export] public float MinPitch = -90f;
	[Export] public float MaxPitch = 90f;
	[Export] public float SpringArmLength = 5f;
	[Export] public bool EnableCollisionAvoidance = true;

	[ExportGroup( "References" )]
	[Export] private Camera3D _camera;
	[Export] private SpringArm3D _springArm;
	[Export] private Node3D _followTarget;

	#endregion

	#region Private Fields

	private Vector3 _targetPosition;
	private Vector3 _currentVelocity = Vector3.Zero;
	private Quaternion _targetRotation = Quaternion.Identity;
	private float _currentPitch = 0f;
	private float _currentYaw = 0f;


	#endregion

	#region Public API

	public override void _Ready()
	{
		InitializeReferences();
		_targetPosition = GlobalPosition;
		_targetRotation = Quaternion;
	}

	public override void _Process( double delta )
	{
		if ( _followTarget != null )
			UpdateTargetPosition();

		UpdateCameraPosition( delta );
		UpdateCameraRotation( delta );
	}

	/// <summary>
	/// Set the target to follow. Pass null to stop following.
	/// </summary>
	public void SetFollowTarget( Node3D target ) => _followTarget = target;

	/// <summary>
	/// Instantly set camera rotation (yaw and pitch in degrees)
	/// </summary>
	public void SetRotation( float yaw, float pitch )
	{
		_currentYaw = yaw;
		_currentPitch = FMath.Clamp( pitch, MinPitch, MaxPitch );
		UpdateRotationQuaternion();
	}

	/// <summary>
	/// Add to current rotation (relative input)
	/// </summary>
	public void AddRotation( float deltaYaw, float deltaPitch )
	{
		_currentYaw += deltaYaw;
		_currentPitch = FMath.Clamp( _currentPitch + deltaPitch, MinPitch, MaxPitch );
		UpdateRotationQuaternion();
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
	public Vector3 GetForwardDirection() => -GlobalTransform.Basis.Z;

	/// <summary>
	/// Get current camera right direction
	/// </summary>
	public Vector3 GetRightDirection() => GlobalTransform.Basis.X;

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
			_targetPosition = _followTarget.GlobalPosition;
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
			// Прямое присваивание кватерниона (градусы → кватернион)
			Quaternion = FMath.FromEulerYXZDegrees( _currentPitch, _currentYaw, 0f );
			return;
		}

		float lerpFactor = FMath.Clamp( (float)delta * RotationLagSpeed, 0f, 1f );
		Quaternion targetQuat = FMath.FromEulerYXZDegrees( _currentPitch, _currentYaw, 0f );
		Quaternion smoothedQuat = Quaternion.FastSlerp( targetQuat, lerpFactor );
		Quaternion = smoothedQuat;
	}

	// This ensures target rotation is updated for next frame interpolation
	private void UpdateRotationQuaternion() => 
		_targetRotation = FMath.FromEulerYXZDegrees( _currentPitch, _currentYaw, 0f );

	#endregion
}