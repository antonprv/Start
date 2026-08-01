// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Framework.FastMath.Godot;

namespace Framework.Components.Camera.Traits.Rotation
{
	/// <summary>
	/// Free-look rotation, unconstrained yaw and clamped pitch, driven directly by
	/// <c>CameraContext.LookInput</c> every frame. This is the standard FPS/third-person
	/// "always follow the mouse" trait.
	/// </summary>
	public class MouseLookTrait : ICameraTrait
	{
		public float MinPitch { get; set; } = -80f;
		public float MaxPitch { get; set; } = 80f;

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			state.TargetYaw += -ctx.LookInput.X;
			state.TargetPitch = FMath.Clamp( state.TargetPitch - ctx.LookInput.Y, MinPitch, MaxPitch );
		}

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion
	}
}
