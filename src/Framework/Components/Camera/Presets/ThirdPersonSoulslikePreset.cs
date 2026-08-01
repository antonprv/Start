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
	/// Elden Ring-style third-person camera - both the framing and the way it moves.
	///
	/// Framing: camera sits high and close to centered behind the character (a small right
	/// offset, not the pronounced shoulder-hug of a modern cover shooter), a longer fixed arm so
	/// the whole character reads clearly, and a narrower pitch range than a typical TPS - you
	/// can look down at your feet but not much past them, and up only moderately.
	///
	/// Movement: no scroll-zoom (Souls cameras don't let you dolly in/out), moderately heavy
	/// rotation/position smoothing (deliberate, not snappy), and - the signature Souls camera
	/// behavior - AutoRecenterTrait: stop touching the camera while running and, after a beat,
	/// it drifts back in behind you on its own.
	///
	/// Deliberately has no CameraLagTrait/CameraOvershootTrait - see GeneralThirdPersonPreset
	/// for this same setup with both added.
	/// </summary>
	internal class ThirdPersonSoulslikePreset : ICameraPreset
	{
		public List<ICameraTrait> Build() => new List<ICameraTrait>()
		{
			new FollowTargetTrait()
			{
				EyeHeight = 1.5f
			},
			new MouseLookTrait()
			{
				MinPitch = -65f,
				MaxPitch = 45f
			},
			new AutoRecenterTrait()
			{
				IdleDelay = 1.0f,
				RecenterSpeed = 40f,
				OnlyWhileMoving = true,
				MinSpeedToRecenter = 0.6f
			},
			new FixedArmLengthTrait()
			{
				ArmLength = 4.2f
			},
			new PoseTrait()
			{
				DefaultOffset = new Vector3( 0.15f, 0.35f, 0f ), // near-centered, elevated - not shoulder-hugging
                DefaultBlendTime = 0.3f
			},
			new SmoothingTrait()
			{
				RotationSmoothSpeed = 10f,  // a bit heavier/more deliberate than the shooter-style shoulder cam
                PositionSmoothSpeed = 9f,
				FOVSmoothSpeed = 8f
			}
		};
	}
}
