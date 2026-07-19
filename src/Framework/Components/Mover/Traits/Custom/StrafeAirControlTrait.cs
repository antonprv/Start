// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.
//
// Air control that blends three references on purpose:
//
//   • MECHANIC (Quake / Half-Life strafing): velocity is projected onto
//     WishDirection and only topped up in that direction — never set
//     directly. This is exactly what makes circle-strafing / strafe-jumping
//     work: turning the camera+input while airborne keeps adding speed
//     instead of just redirecting existing speed. Same core math as
//     QuakeAirStrafeTrait.
//
//   • CURVE (Doom 3 / id Tech "Accelerate"): the amount added per frame is
//     accel * delta * wishspeed (Physics_Player.cpp Accelerate(), the exact
//     formula Doom3AccelerateTrait/Doom3GroundAccelTrait use for the ground
//     case) instead of a flat accel*delta ramp. This is the "ускорение как
//     в Doom 3" half of the request — same shape of curve as the ground
//     trait, just evaluated with air-side tuning values.
//
//   • FEEL (Doom 2016 air control): unlike classic Quake — where AirMaxSpeed
//     is deliberately clamped to ~0.5-1.0 m/s so you're *forced* into
//     strafe-jump technique to gain speed — this trait expects
//     Profile.AirMaxSpeed to be set generously (equal to or above ground
//     MaxSpeed). That single tuning change is what turns "clunky, capped
//     Quake air" into "comfortable, responsive Doom-2016 air": you get
//     near-immediate turning in the air without losing momentum, while the
//     projection mechanic above still rewards good strafe technique with
//     extra speed if you want to push it (see tuning notes on the preset).
//
// Pair with Doom3GroundAccelTrait + Doom3FrictionTrait for ground, NOT with
// Doom3AccelerateTrait (that trait already owns the air case itself, and the
// two would double-apply acceleration in the same frame).

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Custom
{
    [GlobalClass]
    public partial class StrafeAirControlTrait : MovementTraitResource
    {
        public override void PreProcess( ref MovementContext ctx ) { }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            if ( ctx.IsOnFloor )
                return;

            Vector3 wishDir = ctx.WishDirection;
            if ( wishDir.IsNearlyZero() )
                return;

            // Cap comes from AirMaxSpeed, not MaxSpeed — set this generously
            // (>= MaxSpeed) for Doom-2016 comfort, or low (0.5-1.0) to fall
            // back to authentic Quake strafe-jump restriction.
            float wishSpeed = ctx.Profile.AirMaxSpeed;

            float currentSpeed = velocity.FastDot( wishDir );
            float addSpeed = wishSpeed - currentSpeed;
            if ( addSpeed <= 0f )
                return;

            // Doom 3 / q2-style curve: proportional to wishspeed rather than
            // a flat ramp. AirAcceleration here plays the role of
            // PM_AIRACCELERATE (default was 1.0 in the original — this trait
            // takes it from the profile instead so it's tunable per-preset).
            float accelSpeed = ctx.Profile.AirAcceleration * delta * wishSpeed;
            if ( accelSpeed > addSpeed )
                accelSpeed = addSpeed;

            velocity += wishDir * accelSpeed;
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
