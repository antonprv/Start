// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.
//
// Ground-only half of idPhysics_Player::Accelerate() (see Doom3AccelerateTrait
// for the full 1:1 port and source references). Split out so it can be paired
// with a *different* air trait (StrafeAirControlTrait) without both traits
// fighting over the airborne case — Doom3AccelerateTrait handles ground+air
// itself, so don't use both traits in the same preset.
//
// Uses PM_ACCELERATE exactly like the original WalkMove() path.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Custom
{
    [GlobalClass]
    public partial class Doom3GroundAccelTrait : MovementTraitResource
    {
        public override void PreProcess( ref MovementContext ctx ) { }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            if ( !ctx.IsOnFloor )
                return;

            float inputMag = FMath.Min( ctx.WishDirection.FastLength(), 1f );
            if ( inputMag < 0.0001f )
                return;

            Vector3 wishdir = ctx.WishDirection.FastNormalized();
            float wishspeed = ctx.Profile.MaxSpeed * inputMag;

            // q2-style Accelerate() — Physics_Player.cpp lines 123-138.
            float currentspeed = velocity.FastDot( wishdir );
            float addspeed = wishspeed - currentspeed;
            if ( addspeed <= 0f )
                return;

            float accelspeed = Core.Doom3Constants.PM_ACCELERATE * delta * wishspeed;
            if ( accelspeed > addspeed )
                accelspeed = addspeed;

            velocity += accelspeed * wishdir;
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
