// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Interfaces;
using Framework.Components.Mover.Traits.Common;
using Framework.Components.Mover.Traits.Quake;

namespace Framework.Components.Mover.Presets
{
	internal class QuakePreset : IMovementPreset
	{
		public List<IMovementTrait> Build() => new List<IMovementTrait>()
		{
			new GravityTrait(),
			new JumpTrait(),
			new GroundAccelerationTrait(),
			new QuakeAirStrafeTrait(),
			new NoFrictionTrait()
		};

		public void SetDefaultProfile( IMovementProfile profile )
		{
			profile.GroundAcceleration = 25f;
			profile.MaxSpeed = 7f;
			profile.AirAcceleration = 10f;
			profile.AirMaxSpeed = 0.7f;  // low cap = strafe-jump physics
			profile.JumpHeight = 1.5f;
			profile.JumpBufferTime = 0.12f;
			profile.CoyoteTime = 0.12f;
		}
	}
}
