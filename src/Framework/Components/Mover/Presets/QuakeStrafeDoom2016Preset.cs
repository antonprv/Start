// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.
//
// Custom blend requested for the player controller:
//   • Strafing technique:  Quake / Half-Life (projection-cap, momentum kept)
//   • Air control feel:    Doom 2016 (generous, responsive, not "locked in")
//   • Accel curve (ground & air): Doom 3 / id Tech Accelerate() formula
//
// See Traits/Custom/Doom3GroundAccelTrait.cs and
// Traits/Custom/StrafeAirControlTrait.cs for the detailed rationale on each.

using Framework.Components.Mover.Core.Interfaces;
using Framework.Components.Mover.Core.Resources;
using Framework.Components.Mover.Traits.Common;
using Framework.Components.Mover.Traits.Custom;
using Framework.Components.Mover.Traits.Doom3;

namespace Framework.Components.Mover.Presets
{
    public static class QuakeStrafeDoom2016Preset
    {
        public static List<IMovementTrait> Build() => new()
        {
            new GravityTrait(),
            new JumpTrait(),               // buffered + coyote - swap for Doom3JumpTrait
                                            // if you want the exact no-buffer Doom 3 jump feel
            new Doom3FrictionTrait(),       // ground stopping friction, PM_FRICTION (air friction = 0, harmless here)
            new Doom3GroundAccelTrait(),    // ground accel, PM_ACCELERATE curve
            new StrafeAirControlTrait()     // air: Quake strafe mechanic + Doom3 curve + generous cap
        };

        public static MovementProfile DefaultProfile() => new()
        {
            // ── Ground (Doom 3 curve) ──────────────────────────────
            GroundAcceleration = 0f,   // unused - Doom3GroundAccelTrait owns this via PM_ACCELERATE
            MaxSpeed = 7f,             // your run speed, m/s - tune to taste
            GroundFriction = 0f,       // unused - Doom3FrictionTrait owns this via PM_FRICTION

            // ── Air (Quake mechanic, Doom3 curve, Doom2016 cap) ────
            // AirAcceleration plays PM_AIRACCELERATE's role (original default 1.0,
            // in "curve strength" units, not m/s^2 - it's multiplied by wishspeed
            // each frame). Push this up for snappier turning.
            AirAcceleration = 6f,

            // THIS is the main knob between "authentic Quake" and "Doom 2016 comfort":
            //   0.5 – 1.0   → classic strafe-jump restriction, forces circle-strafe technique
            //   = MaxSpeed  → full, comfortable air control (Doom 2016 feel) - recommended default
            //   > MaxSpeed  → still comfortable, plus some bunny-hop-style overspeed via good strafing
            AirMaxSpeed = 9f,

            AirControl = 0f,           // unused by this preset

            // ── Jump ────────────────────────────────────────────────
            JumpHeight = 1.4f,
            JumpBufferTime = 0.12f,
            CoyoteTime = 0.12f
        };
    }
}
