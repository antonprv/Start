// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.
//
// Ported 1:1 from idPhysics_Player::Accelerate() — Physics_Player.cpp lines 123-138
// (the "#if 1 // q2 style" branch, which is what ships).
// Called from WalkMove() with PM_ACCELERATE and from AirMove() with
// PM_AIRACCELERATE — Physics_Player.cpp lines 606-638 (AirMove) and
// 646-732 (WalkMove). Ground/slick/knockback accel-swap (line 704-709) is
// deferred to Phase 2 along with surface flags.
//
// NOTE on wishspeed: the original derives wishspeed from CmdScale(), which
// scales by raw forwardmove/rightmove analog axes so that diagonal input
// doesn't exceed straight-line input speed. Our MovementContext only carries
// a single combined WishDirection vector (already camera-relative), so
// wishspeed here is MaxSpeed * |WishDirection| (clamped to 1) — the closest
// equivalent available without adding raw axis data to the context.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Doom3
{
    [GlobalClass]
    public partial class Doom3AccelerateTrait : MovementTraitResource
    {
        public override void PreProcess( ref MovementContext ctx ) { }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            float inputMag = FMath.Min( ctx.WishDirection.FastLength(), 1f );
            if ( inputMag < 0.0001f )
                return;

            Vector3 wishdir = ctx.WishDirection.FastNormalized();
            float wishspeed = ctx.Profile.MaxSpeed * inputMag;

            float accel = ctx.IsOnFloor
                ? Core.Doom3Constants.PM_ACCELERATE
                : Core.Doom3Constants.PM_AIRACCELERATE;

            Accelerate( wishdir, wishspeed, accel, ref velocity, delta );
        }

        /// <summary>Direct port of idPhysics_Player::Accelerate — q2-style, lines 123-138.</summary>
        private static void Accelerate( Vector3 wishdir, float wishspeed, float accel, ref Vector3 velocity, float frametime )
        {
            float currentspeed = velocity.FastDot( wishdir );
            float addspeed = wishspeed - currentspeed;
            if ( addspeed <= 0f )
                return;

            float accelspeed = accel * frametime * wishspeed;
            if ( accelspeed > addspeed )
                accelspeed = addspeed;

            velocity += accelspeed * wishdir;
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
