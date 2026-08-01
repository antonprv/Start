// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Camera.Traits.Pan
{
	/// <summary>
	/// Moves the rig away from its pivot in world-space XZ, driven by <c>ctx.PanInput</c>
	/// (cursor-to-edge distance, filled in by CameraComponent). The pan direction is rotated by
	/// the rig's current yaw so pushing the cursor to the top of the screen always pans "up the
	/// screen" regardless of camera orientation - the standard RTS/Diablo edge-scroll feel.
	///
	/// <see cref="MaxPanRadius"/> acts as a soft leash back to whatever FollowTargetTrait is
	/// tracking, so the camera can wander to look around but won't get lost - set it very large
	/// (or the trait to disabled) for a fully free, unleashed camera.
	/// </summary>
	public class EdgeScrollPanTrait : ICameraTrait
	{
		#region Properties

		public float PanSpeed { get; set; } = 12f;

		/// <summary>Max distance the pan offset may reach from the pivot. &lt;= 0 means unlimited.</summary>
		public float MaxPanRadius { get; set; } = 20f;

		#endregion

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			if ( ctx.PanInput.LengthSq() < 0.0001f )
				return;

			float yawRad = state.TargetYaw * FMath.Deg2Rad;
			Basis yawBasis = new Basis( FMath.FromAxisAngle( Vector3.Up, yawRad ) );

			Vector3 screenRelative = new Vector3( ctx.PanInput.X, 0f, -ctx.PanInput.Y );
			Vector3 worldPan = yawBasis * screenRelative;

			state.PanOffset += worldPan * PanSpeed * delta;

			if ( MaxPanRadius > 0f && state.PanOffset.FastLength() > MaxPanRadius )
				state.PanOffset = state.PanOffset.FastNormalized() * MaxPanRadius;
		}

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion
	}
}
