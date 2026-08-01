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
	/// Fixed, angled top-down camera that follows the character - the classic Diablo/isometric
	/// ARPG look. Pitch and yaw are pinned (no player rotation input at all); only position is
	/// smoothed, giving a steady, professional "the camera just tracks you" feel.
	/// </summary>
	internal class TopDownFixedPreset : ICameraPreset
	{
		public List<ICameraTrait> Build() => new List<ICameraTrait>()
		{
			new FollowTargetTrait()
			{
				EyeHeight = 0f
			},
			new FixedAngleTrait()
			{
				FixedPitch = -55f,
				FixedYaw = 45f,
				AllowYawInput = false
			},
			new FixedArmLengthTrait()
			{
				ArmLength = 14f
			},
			new PoseTrait()
			{
                // keeps offset reset to 0 across mode hot-swaps
                DefaultOffset = Vector3.Zero
			},
			new SmoothingTrait()
			{
				RotationSmoothSpeed = 0f,   // angle is pinned, nothing to smooth
                PositionSmoothSpeed = 6f,
				FOVSmoothSpeed = 8f
			}
		};
	}
}
