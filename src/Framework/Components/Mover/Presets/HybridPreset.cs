// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Interfaces;
using Framework.Components.Mover.Traits.Common;
using Framework.Components.Mover.Traits.Hybrid;

namespace Framework.Components.Mover.Presets
{
	//   - Ground: lerp directly toward target velocity - instant feel, no friction needed
	//   - Air: symmetric lerp control, responsive
	//   - No friction trait needed - lerp handles both acceleration and deceleration
	internal class HybridPreset : IMovementPreset
	{
		public List<IMovementTrait> Build() => new List<IMovementTrait>()
		{
			new GravityTrait(),
			new JumpTrait(),
			new HybridGroundTrait(),
			new HybridAirControlTrait()
		};

		public void SetDefaultProfile( IMovementProfile profile )
		{
			profile.GroundAcceleration = 14f;   // lerp speed on ground
			profile.MaxSpeed = 6f;
			profile.AirMaxSpeed = 5f;
			profile.AirControl = 5f;   // lerp speed in air
			profile.JumpHeight = 1.2f;
			profile.JumpBufferTime = 0.15f;
			profile.CoyoteTime = 0.15f;
		}
	}
}
