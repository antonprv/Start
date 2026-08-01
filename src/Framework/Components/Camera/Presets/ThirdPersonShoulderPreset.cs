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
	/// Modern over-the-shoulder third-person camera (Gears of War / RE4-style): free mouse
	/// look, a scrollable arm, an over-shoulder pose you can retarget at runtime via
	/// CameraComponent.TransitionToPose (e.g. crouch/stealth vs. normal walk), plus lag and
	/// overshoot for a bit of weight and juice.
	/// </summary>
	internal class ThirdPersonShoulderPreset : ICameraPreset
	{
		public List<ICameraTrait> Build() => new List<ICameraTrait>()
		{
			new FollowTargetTrait()
			{
				EyeHeight = 1.5f
			},
			new MouseLookTrait
			{
				MinPitch = -60f,
				MaxPitch = 75f
			},
			new ScrollZoomTrait()
			{
				InitialArmLength = 3.5f,
				MinArmLength = 1.5f,
				MaxArmLength = 6f
			},
			new PoseTrait()
			{
				DefaultOffset = new Vector3( 0.55f, 0.25f, 0f ), // classic over-the-shoulder framing
                DefaultBlendTime = 0.35f
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
				RotationSmoothSpeed = 18f,
				PositionSmoothSpeed = 10f,
				FOVSmoothSpeed = 8f
			}
		};
	}
}
