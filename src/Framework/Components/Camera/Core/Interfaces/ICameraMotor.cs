// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Framework.Components.Camera.Core.Interfaces
{
	public interface ICameraMotor
	{
		/// <summary>The persistent, accumulated rig state - the camera equivalent of Velocity.</summary>
		CameraRigState State { get; set; }

		void Simulate( float delta, CameraContext context );

		/// <summary>
		/// Hot-swap the trait list at runtime. State is preserved - only behavior changes,
		/// exactly like <c>IMovementMotor.SetTraits</c>. This is what lets a character switch
		/// from, say, a shoulder camera to a fixed top-down camera without a visible pop:
		/// Yaw/Pitch/ArmLength/FOV all carry over and simply blend toward whatever the new
		/// traits ask for.
		/// </summary>
		void SetTraits( List<ICameraTrait> traits );
	}
}
