// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;

namespace Framework.Components.Camera.Traits.Feel
{
	/// <summary>
	/// The final "settle" trait most presets should end with: continuously eases Yaw/Pitch,
	/// ArmLength, LocalOffset and FOV toward their Target* counterparts. Any of the four speeds
	/// can be set to 0 for an instant, unsmoothed snap on that channel - e.g. a Diablo camera
	/// might want smoothed position but a completely rigid pitch.
	///
	/// Put this after every other trait so it smooths whatever they've asked for, and before
	/// nothing except CameraOvershootTrait/CameraLagTrait if you want their transient distance
	/// pushed on top of already-smoothed values (recommended order: rotation/pan/distance/pose
	/// traits -> lag -> overshoot -> smoothing is also valid; both orderings look fine in
	/// practice since Overshoot/Lag are purely additive on separate fields).
	/// </summary>
	public class SmoothingTrait : ICameraTrait
	{
		#region Properties

		/// <summary>0 = snap instantly to TargetYaw/TargetPitch.</summary>
		public float RotationSmoothSpeed { get; set; } = 15f;

		/// <summary>0 = snap instantly to TargetArmLength/TargetLocalOffset.</summary>
		public float PositionSmoothSpeed { get; set; } = 10f;

		/// <summary>0 = snap instantly to TargetFOV.</summary>
		public float FOVSmoothSpeed { get; set; } = 8f;

		#endregion

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			float rt = RotationSmoothSpeed <= 0f ? 1f : FMath.Clamp01( delta * RotationSmoothSpeed );
			state.Yaw = state.Yaw.LerpAngle( state.TargetYaw, rt );
			state.Pitch = FMath.Lerp( state.Pitch, state.TargetPitch, rt );

			float pt = PositionSmoothSpeed <= 0f ? 1f : FMath.Clamp01( delta * PositionSmoothSpeed );
			state.ArmLength = FMath.Lerp( state.ArmLength, state.TargetArmLength, pt );
			state.LocalOffset = state.LocalOffset.FastLerp( state.TargetLocalOffset, pt );

			float ft = FOVSmoothSpeed <= 0f ? 1f : FMath.Clamp01( delta * FOVSmoothSpeed );
			state.FOV = FMath.Lerp( state.FOV, state.TargetFOV, ft );
		}

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion
	}
}
