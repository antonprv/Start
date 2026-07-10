// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Framework.FastMath;
using Godot;

namespace Framework.Components.Mover.Traits.Hybrid
{
    /// <summary>
    /// Lerp-based ground movement.
    /// Directly steers horizontal velocity toward (wishDir * MaxSpeed) each frame.
    /// Produces the snappy, arcade feel of games like Celeste or platformer Mario games.
    /// </summary>
    [GlobalClass]
    public partial class HybridGroundTrait : MovementTraitResource
    {
        public override void PreProcess( ref MovementContext ctx ) { }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            if ( !ctx.IsOnFloor )
                return;

            Vector3 target = ctx.WishDirection * ctx.Profile.MaxSpeed;
            Vector3 horizontal = new Vector3( velocity.X, 0f, velocity.Z );

            horizontal = horizontal.FastLerp( target, ctx.Profile.GroundAcceleration * delta );

            velocity.X = horizontal.X;
            velocity.Z = horizontal.Z;
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
