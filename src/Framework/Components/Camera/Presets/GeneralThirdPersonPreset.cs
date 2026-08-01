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
	/// The default third-person camera for the game right now (see CameraComponent.InitialMode).
	///
	/// Exactly ThirdPersonSoulslikePreset's framing and auto-recenter movement, with
	/// CameraLagTrait and CameraOvershootTrait layered on top: the camera visibly falls behind
	/// on a hard sprint and eases back in once the character stops, and whipping the camera
	/// around fast makes it fly a bit further out on its arm before easing back to its normal
	/// distance - on top of the same Elden Ring-style base feel.
	/// </summary>
	internal class GeneralThirdPersonPreset : ICameraPreset
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
				DefaultOffset = new Vector3( 0.15f, 0.35f, 0f ),
				DefaultBlendTime = 0.3f
			},
			new CameraLagTrait()
			{
				LagPerSpeed = 0.15f,
				MaxLagDistance = 2.5f,
				BuildSpeed = 8f,
				RecoverSpeed = 2f
			},
			new CameraOvershootTrait()
			{
				OvershootPerDegree = 0.035f,
				MaxOvershootDistance = 1.2f,
				BuildSpeed = 10f,
				RecoverSpeed = 3f
			},
			new SmoothingTrait()
			{
				RotationSmoothSpeed = 10f,
				PositionSmoothSpeed = 9f,
				FOVSmoothSpeed = 8f
			}
		};
	}
}
