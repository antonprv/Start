// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Interfaces;
using Framework.Components.Mover.Traits.Common;
using Framework.Components.Mover.Traits.Custom;
using Framework.Components.Mover.Traits.Doom3;

namespace Framework.Components.Mover.Presets
{
	// Custom blend requested for the player controller:
	//   - Strafing technique:  Quake / Half-Life
	//   - Air control feel:    Doom 2016
	//   - Accel curve (ground & air): Doom 3 / id Tech Accelerate() formula
	internal class QuakeStrafeDoom2016Preset : IMovementPreset
	{
		public List<IMovementTrait> Build() => new List<IMovementTrait>()
		{
			new GravityTrait(),
			new JumpTrait(),                // buffered + coyote - swap for Doom3JumpTrait
            new Doom3FrictionTrait(),       // ground stopping friction, PM_FRICTION (air friction = 0, harmless here)
            new Doom3GroundAccelTrait(),    // ground accel, PM_ACCELERATE curve
            new StrafeAirControlTrait()     // air: Quake strafe mechanic + Doom3 curve + generous cap
        };

		public void SetDefaultProfile( IMovementProfile profile )
		{
			profile.MaxSpeed = 7f;
			profile.AirAcceleration = 6f;
			profile.AirMaxSpeed = 9f;
			profile.JumpHeight = 1.4f;
			profile.JumpBufferTime = 0.12f;
			profile.CoyoteTime = 0.12f;
		}
	}
}
