// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Realistic
{
    /// <summary>
    /// Ground acceleration for the Realistic preset.
    /// Uses a simple additive impulse capped by MaxSpeed, similar to Source engine.
    /// Works in tandem with GroundFrictionTrait: friction decelerates, this accelerates.
    /// </summary>
    [GlobalClass]
    public partial class RealisticGroundAccelTrait : MovementTraitResource
    {
        public override void PreProcess( ref MovementContext ctx ) { }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            if ( !ctx.IsOnFloor )
                return;

            Vector3 wishDir = ctx.WishDirection;
            if ( wishDir.IsNearlyZero() )
                return;

            Vector3 horizontal = new Vector3( velocity.X, 0f, velocity.Z );
            float currentSpeed = horizontal.FastDot( wishDir );
            float addSpeed = ctx.Profile.MaxSpeed - currentSpeed;

            if ( addSpeed <= 0f )
                return;

            float accel = ctx.Profile.GroundAcceleration * delta;
            if ( accel > addSpeed )
                accel = addSpeed;

            velocity.X += wishDir.X * accel;
            velocity.Z += wishDir.Z * accel;
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
