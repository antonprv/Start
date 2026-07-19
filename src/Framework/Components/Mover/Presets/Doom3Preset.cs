// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.
//
// Wires up the Phase 1 (ground/air walk) port of idPhysics_Player.
// Order mirrors idPhysics_Player::MovePlayer() dispatch for PM_NORMAL:
// CheckJump before Friction/Accelerate inside WalkMove, gravity integration
// inside SlideMove. Our pipeline runs PreProcess -> Process(in list order)
// -> PostProcess, so trait order below reproduces call order:
//   CheckJump (may consume the jump) -> Friction -> Accelerate -> Gravity.
//
// NOT YET PORTED (Phase 2+, see project roadmap):
//   WaterMove / WaterJumpMove / LadderMove / CheckDuck / CheckWaterJump,
//   slick-surface + knockback accel swap, step-up/step-down sliding,
//   idPhysics_RigidBody, idPhysics_AF (ragdolls), idPhysics_Monster,
//   idPhysics_Parametric.

using Framework.Components.Mover.Core.Interfaces;
using Framework.Components.Mover.Core.Resources;
using Framework.Components.Mover.Traits.Common;
using Framework.Components.Mover.Traits.Doom3;

namespace Framework.Components.Mover.Presets
{
    /// <summary>
    /// Doom 3 (idPhysics_Player) movement preset — Phase 1: ground + air walk.
    /// Exact PM_* constants from the original, see Core.Doom3Constants.
    /// </summary>
    public static class Doom3Preset
    {
        public static List<IMovementTrait> Build() => new()
        {
            new Doom3JumpTrait(),
            new Doom3FrictionTrait(),
            new Doom3AccelerateTrait(),
            new GravityTrait()
        };

        /// <summary>
        /// MaxSpeed here should be set to your walkSpeed cvar equivalent
        /// (Doom 3 default pm_walkspeed = 76 inches/sec ≈ 1.93 m/s at 1:1 scale
        /// via Doom3Constants.InchesToMeters — feels slow because Doom 3's
        /// "run" is actually the default; pm_speed / sprint is a separate cvar
        /// not ported yet).
        /// </summary>
        public static MovementProfile DefaultProfile() => new()
        {
            GroundAcceleration = 0f, // unused — Doom3AccelerateTrait owns this
            MaxSpeed = 76f * Core.Doom3Constants.InchesToMeters,
            GroundFriction = 0f,     // unused — Doom3FrictionTrait owns this
            AirAcceleration = 0f,    // unused — Doom3AccelerateTrait owns this
            AirMaxSpeed = 0f,        // unused
            AirControl = 0f,         // unused
            JumpHeight = 0f,          // unused — Doom3JumpTrait computes from height
            JumpBufferTime = 0f,     // unused — no buffering in the original
            CoyoteTime = 0f          // unused — no coyote time in the original
        };
    }
}
