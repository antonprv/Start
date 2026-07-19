// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Interfaces;
using Framework.Components.Mover.Core.Resources;
using Framework.Components.Mover.Traits.Common;
using Framework.Components.Mover.Traits.Quake;

namespace Framework.Components.Mover.Presets
{
    /// <summary>
    /// Quake / Half-Life style movement preset.
    ///
    /// Characteristics:
    ///   • Ground: direct impulse acceleration, near-zero friction
    ///   • Air: projection-capped strafe acceleration (bunny-hop friendly)
    ///   • Momentum: fully preserved — no automatic braking
    ///
    /// Key tuning lever: AirMaxSpeed on the profile.
    ///   0.5–1.0 → authentic strafe-jump feel
    ///   3.0+    → fast but no exotic speed gains
    /// </summary>
    public static class QuakePreset
    {
        public static List<IMovementTrait> Build() => new()
        {
            new GravityTrait(),
            new JumpTrait(),
            new GroundAccelerationTrait(),
            new QuakeAirStrafeTrait(),
            new NoFrictionTrait()
        };

        public static MovementProfile DefaultProfile() => new()
        {
            GroundAcceleration = 25f,
            MaxSpeed = 7f,
            GroundFriction = 0f,    // unused in this preset
            AirAcceleration = 10f,
            AirMaxSpeed = 0.7f,  // low cap = strafe-jump physics
            AirControl = 0f,    // unused in this preset
            JumpHeight = 1.5f,
            JumpBufferTime = 0.12f,
            CoyoteTime = 0.12f
        };
    }
}
