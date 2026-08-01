// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core.Interfaces;
using Framework.Components.Camera.Traits.Distance;
using Framework.Components.Camera.Traits.Feel;
using Framework.Components.Camera.Traits.Follow;
using Framework.Components.Camera.Traits.Pan;
using Framework.Components.Camera.Traits.Pose;
using Framework.Components.Camera.Traits.Rotation;

using Godot;

namespace Framework.Components.Camera.Presets
{
	/// <summary>
	/// Fixed-angle top-down camera that's moved entirely by pushing the cursor to the edge of
	/// the screen - classic RTS scrolling. FollowTargetTrait is kept (with a generous leash
	/// radius) purely so the camera doesn't wander off into the void forever; set
	/// EdgeScrollPanTrait.MaxPanRadius &lt;= 0 or FollowTargetTrait.Enabled = false for a
	/// completely untethered free camera.
	/// </summary>
	internal class EdgeScrollTopDownPreset : ICameraPreset
	{
		public List<ICameraTrait> Build() => new List<ICameraTrait>()
		{
			new FollowTargetTrait()
			{
				EyeHeight = 0f
			},
			new FixedAngleTrait()
			{
				FixedPitch = -60f,
				FixedYaw = 0f,
				AllowYawInput = false
			},
			new EdgeScrollPanTrait()
			{
				PanSpeed = 18f,
				MaxPanRadius = 40f
			},
			new FixedArmLengthTrait()
			{
				ArmLength = 16f
			},
			new PoseTrait()
			{
                // keeps offset reset to 0 across mode hot-swaps
                DefaultOffset = Vector3.Zero
			},
			new SmoothingTrait()
			{
				RotationSmoothSpeed = 0f,
				PositionSmoothSpeed = 14f,
				FOVSmoothSpeed = 8f
			}
		};
	}
}
