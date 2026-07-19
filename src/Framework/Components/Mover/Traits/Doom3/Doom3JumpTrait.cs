// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.
//
// Ported 1:1 from idPhysics_Player::CheckJump() — Physics_Player.cpp lines 1181-1208.
//
//   addVelocity = 2.0f * maxJumpHeight * -gravityVector;
//   addVelocity *= idMath::Sqrt( addVelocity.Normalize() );   // idVec3::Normalize()
//                                                              // returns the pre-normalize
//                                                              // length, then normalizes in place
//   current.velocity += addVelocity;
//
// which reduces to the closed-form projectile equation v = sqrt(2 * g * h) along
// the up axis — Doom 3 is height-based, not speed-based, unlike the stock
// JumpTrait in this project (which just sets velocity.Y = Profile.JumpSpeed).
//
// Default pm_jumpheight cvar = 48 (Doom units/inches) — game/gamesys/SysCvar.cpp line 230.
// No jump buffer / coyote time in the original: only a "must release jump key
// before jumping again" debounce (PMF_JUMP_HELD), reproduced below via
// ctx.JumpRequested edge-detection instead of a held-button flag, since our
// input context only exposes a "requested this frame" pulse.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Doom3
{
    [GlobalClass]
    public partial class Doom3JumpTrait : MovementTraitResource
    {
        /// <summary>Mirrors pm_jumpheight — Doom units (inches). Default matches the original's "48".</summary>
        [Export] public float MaxJumpHeightInches { get; set; } = 48f;

        public override void PreProcess( ref MovementContext ctx ) { }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            // "don't jump if we can't stand up" (PMF_DUCKED) is deferred to Phase 2
            // (CheckDuck isn't ported yet), so only the ground + input-pulse gate applies here.
            if ( !ctx.IsOnFloor || !ctx.JumpRequested )
                return;

            float g = FMath.Max( ctx.Gravity.FastLength(), 0.0001f );
            float h = MaxJumpHeightInches * Core.Doom3Constants.InchesToMeters;

            float jumpSpeed = FMath.FastSqrt( 2f * h * g );
            velocity.Y = jumpSpeed;

            ctx.JumpConsumed = true;
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
