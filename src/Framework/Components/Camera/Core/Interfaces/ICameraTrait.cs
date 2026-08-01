// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Framework.Components.Camera.Core.Interfaces
{
	/// <summary>
	/// A single, composable slice of camera behavior - the camera equivalent of
	/// <c>IMovementTrait</c>. A camera "feel" is built by combining several of these
	/// (look input, follow, distance, lag, overshoot, ...) into an ordered list that a
	/// <see cref="ICameraMotor"/> runs every frame.
	///
	/// Traits never touch the scene tree directly - they only read <see cref="CameraContext"/>
	/// (this frame's input/target snapshot) and mutate <see cref="CameraRigState"/> (the
	/// persistent, accumulated rig state). The owning CameraComponent is the only thing that
	/// converts the final state into an actual Node3D/Camera3D/SpringArm3D transform.
	/// </summary>
	public interface ICameraTrait
	{
		/// <summary>
		/// Runs before the state is touched. Use for timers, edge-detection, buffering -
		/// anything that needs to observe the context before other traits mutate state.
		/// </summary>
		void PreProcess( ref CameraContext ctx );

		/// <summary>Reads/writes the accumulated rig state for this trait's slice of behavior.</summary>
		void Process( ref CameraContext ctx, ref CameraRigState state, float delta );

		/// <summary>Runs after every trait has processed. Use for clamping or cross-trait cleanup.</summary>
		void PostProcess( ref CameraContext ctx );
	}
}
