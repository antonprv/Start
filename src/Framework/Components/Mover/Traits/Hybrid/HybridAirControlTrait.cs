// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Framework.FastMath;
using Godot;

namespace Framework.Components.Mover.Traits.Hybrid
{
    /// <summary>
    /// Lerp-based air control, symmetric with HybridGroundTrait.
    /// Full directional control in the air, slowed by AirControl factor.
    /// </summary>
    [GlobalClass]
    public partial class HybridAirControlTrait : MovementTraitResource
    {
        public override void PreProcess( ref MovementContext ctx ) { }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            if ( ctx.IsOnFloor )
                return;

            Vector3 wishDir = ctx.WishDirection;
            if ( wishDir.IsNearlyZero() )
                return;

            Vector3 horizontal = new Vector3( velocity.X, 0f, velocity.Z );
            Vector3 target = wishDir * ctx.Profile.AirMaxSpeed;

            horizontal = horizontal.FastLerp( target, ctx.Profile.AirControl * delta );

            velocity.X = horizontal.X;
            velocity.Z = horizontal.Z;
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
