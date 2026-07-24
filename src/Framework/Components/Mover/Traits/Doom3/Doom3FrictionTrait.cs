// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.
//
// Ported 1:1 from idPhysics_Player::Friction() — Physics_Player.cpp lines 440-495.
// Water/slick-surface branches are stubbed (return same as air) until the
// Phase 2 (ladder + water) pass wires up WaterLevel / surface flags into
// MovementContext. Ground and air friction match the original exactly.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Doom3
{
    [GlobalClass]
    public partial class Doom3FrictionTrait : MovementTraitResource
    {
        public override void PreProcess( ref MovementContext ctx ) { }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            // vel = current.velocity; if (walking) vel += (vel*gravityNormal)*gravityNormal;
            // — gravityNormal is (0,-1,0) in our up-is-+Y world, so the "ignore slope
            // movement" step from the original collapses to zeroing Y when grounded.
            Vector3 vel = velocity;
            if ( ctx.IsOnFloor )
                vel.Y = 0f;

            float speed = vel.FastLength();
            if ( speed < 1.0f * Core.Doom3Constants.InchesToMeters )
            {
                // remove all movement orthogonal to gravity (lets the player sink)
                if ( FMath.AbsBranchless( velocity.Y ) < 1e-5f )
                    velocity = Vector3.Zero;
                else
                    velocity = new Vector3( 0f, velocity.Y, 0f );
                return;
            }

            float drop = 0f;
            float k = Core.Doom3Constants.InchesToMeters;

            if ( ctx.IsOnFloor )
            {
                // TODO Phase 2: skip this branch on SURF_SLICK / PMF_TIME_KNOCKBACK
                float stop = Core.Doom3Constants.PM_STOPSPEED * k;
                float control = speed < stop ? stop : speed;
                drop += control * Core.Doom3Constants.PM_FRICTION * delta;
            }
            else
            {
                // air friction is 0.0f in the original - kept explicit for parity
                drop += speed * Core.Doom3Constants.PM_AIRFRICTION * delta;
            }

            float newSpeed = speed - drop;
            if ( newSpeed < 0f )
                newSpeed = 0f;

            velocity *= newSpeed / speed;
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
