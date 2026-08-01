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
	/// FirstPersonPreset plus CameraOvershootTrait: whip the mouse around fast and the camera
	/// pulls back a touch from its normal zero-length arm before easing back in - the same
	/// "Metro Gravity"-style inertia used by the third-person presets, just kept very subtle
	/// here since a first-person camera IS the player's head and a big pull-back would look like
	/// clipping rather than weight.
	/// </summary>
	internal class FirstPersonOvershootPreset : ICameraPreset
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
                // keeps offset reset to 0 when hot-swapped in from another mode
                DefaultOffset = Vector3.Zero
			},
			new CameraOvershootTrait()
			{
				OvershootPerDegree = 0.01f,
				MaxOvershootDistance = 0.15f,
				BuildSpeed = 12f,
				RecoverSpeed = 5f
			},
			new SmoothingTrait()
			{
				RotationSmoothSpeed = 0f, // instant look - overshoot only affects distance, not rotation
                PositionSmoothSpeed = 25f,
				FOVSmoothSpeed = 10f
			}
		};
	}
}
