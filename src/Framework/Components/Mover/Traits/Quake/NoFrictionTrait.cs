// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Framework.FastMath;
using Godot;

namespace Framework.Components.Mover.Traits.Quake
{
    /// <summary>
    /// Near-zero ground friction for Quake-style movement.
    /// Applies only a tiny passive damping when the player has no input.
    /// With active input, GroundAccelerationTrait handles everything and
    /// this trait does nothing — so the player keeps all momentum.
    /// </summary>
    [GlobalClass]
    public partial class NoFrictionTrait : MovementTraitResource
    {
        [Export] public float PassiveDamping { get; set; } = 0.98f;

        public override void PreProcess( ref MovementContext ctx ) { }

        public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
        {
            if ( !ctx.IsOnFloor )
                return;

            // Active input — full momentum preserved
            if ( ctx.WishDirection.FastLength() > 0.01f )
                return;

            velocity.X *= PassiveDamping;
            velocity.Z *= PassiveDamping;
        }

        public override void PostProcess( ref MovementContext ctx ) { }
    }
}
