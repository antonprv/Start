// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Interfaces;
using Framework.Components.Mover.Traits.Common;
using Framework.Components.Mover.Traits.Realistic;

namespace Framework.Components.Mover.Presets
{
	internal class RealisticPreset : IMovementPreset
	{
		public List<IMovementTrait> Build() => new List<IMovementTrait>()
		{
			new GravityTrait(),
			new JumpTrait(),
			new GroundFrictionTrait(),
			new RealisticGroundAccelTrait(),
			new SmoothStopTrait(),
			new ClampedAirControlTrait()
		};

		public void SetDefaultProfile( IMovementProfile profile )
		{
			profile.GroundAcceleration = 20f;
			profile.MaxSpeed = 5f;
			profile.GroundFriction = 10f;
			profile.AirAcceleration = 4f;
			profile.AirMaxSpeed = 4f;
			profile.AirControl = 1.5f;
			profile.JumpHeight = 1.2f;
			profile.JumpBufferTime = 0.10f;
			profile.CoyoteTime = 0.10f;
		}
	}
}
