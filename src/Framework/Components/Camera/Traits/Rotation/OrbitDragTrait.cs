// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Framework.FastMath.Godot;

namespace Framework.Components.Camera.Traits.Rotation
{
	/// <summary>
	/// Rotates yaw/pitch only while <c>ctx.OrbitHeld</c> is true (e.g. RMB held) - the
	/// click-and-drag orbit behavior used by Baldur's Gate 3 and most isometric CRPGs. When the
	/// button isn't held, the camera keeps whatever angle it last had, so it doesn't snap around
	/// just because the mouse moved.
	/// </summary>
	public class OrbitDragTrait : ICameraTrait
	{
		#region Properties

		public float MinPitch { get; set; } = -80f;
		public float MaxPitch { get; set; } = -15f;

		#endregion

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			if ( !ctx.OrbitHeld )
				return;

			state.TargetYaw += -ctx.LookInput.X;
			state.TargetPitch = FMath.Clamp( state.TargetPitch - ctx.LookInput.Y, MinPitch, MaxPitch );
		}

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion
	}
}
