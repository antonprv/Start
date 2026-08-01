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
	/// Rotatable, zoomable isometric-ish orbit camera - Baldur's Gate 3-style. Yaw/pitch only
	/// change while the orbit button is held (click-and-drag), the mouse-wheel zooms the arm,
	/// and pushing the cursor to the screen edge pans the camera around within a leash radius of
	/// the tracked party member/character.
	/// </summary>
	internal class TopDownOrbitPreset : ICameraPreset
	{
		public List<ICameraTrait> Build() => new List<ICameraTrait>()
		{
			new FollowTargetTrait()
			{
				EyeHeight = 0f
			},
			new OrbitDragTrait()
			{
				MinPitch = -80f,
				MaxPitch = -20f
			},
			new EdgeScrollPanTrait()
			{
				PanSpeed = 14f,
				MaxPanRadius = 20f
			},
			new ScrollZoomTrait()
			{
				InitialArmLength = 12f,
				MinArmLength = 6f,
				MaxArmLength = 25f
			},
			new PoseTrait()
			{
                // keeps offset reset to 0 across mode hot-swaps
                DefaultOffset = Vector3.Zero
			},
			new SmoothingTrait()
			{
				RotationSmoothSpeed = 12f,
				PositionSmoothSpeed = 8f,
				FOVSmoothSpeed = 8f
			}
		};
	}
}
