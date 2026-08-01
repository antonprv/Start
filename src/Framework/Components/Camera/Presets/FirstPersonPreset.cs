// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core.Interfaces;
using Framework.Components.Camera.Traits.Distance;
using Framework.Components.Camera.Traits.Feel;
using Framework.Components.Camera.Traits.Follow;
using Framework.Components.Camera.Traits.Pose;
using Framework.Components.Camera.Traits.Rotation;
using Godot;

namespace Framework.Components.Camera.Presets
{
	/// <summary>
	/// Classic first-person camera: eye-height pivot, free unconstrained mouse look, and a
	/// zero-length arm so the "spring arm" degenerates to sitting right at the pivot.
	/// Smoothing is set fast/near-instant since FPS cameras should feel immediate.
	/// </summary>
	internal class FirstPersonPreset : ICameraPreset
	{
		public List<ICameraTrait> Build() => new List<ICameraTrait>()
		{
			new FollowTargetTrait()
			{
				EyeHeight = 1.7f
			},
			new MouseLookTrait()
			{
				MinPitch = -85f,
				MaxPitch = 85f
			},
			new FixedArmLengthTrait()
			{
				ArmLength = 0f
			},
			new PoseTrait()
			{
                // resets offset to 0 if hot-swapped in from a third-person mode
                DefaultOffset = Vector3.Zero
			},
			new SmoothingTrait()
			{
				RotationSmoothSpeed = 0f,   // instant - no perceptible smoothing lag on look
                PositionSmoothSpeed = 25f,
				FOVSmoothSpeed = 10f
			}
		};
	}
}
