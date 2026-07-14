// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Realistic
{
    /// <summary>
    /// Proportional ground friction — scales velocity down each frame.
    /// Runs on ground regardless of input, so it fights alongside any
    /// acceleration trait (CS / Source engine style).
    /// </summary>
    [GlobalClass]
    public partial class GroundFrictionTrait : MovementTraitResource
    {
        public override void PreProcess( ref MovementContext ctx ) { }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            if ( !ctx.IsOnFloor )
                return;

            float speed = new Vector3( velocity.X, 0f, velocity.Z ).FastLength();
            if ( speed < 0.001f )
                return;

            float drop = speed * ctx.Profile.GroundFriction * delta;
            float newSpeed = FMath.Max( speed - drop, 0f );
            float scale = newSpeed / speed;

            velocity.X *= scale;
            velocity.Z *= scale;
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
