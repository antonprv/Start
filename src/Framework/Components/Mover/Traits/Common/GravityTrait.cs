// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Godot;

namespace Framework.Components.Mover.Traits.Common
{
    [GlobalClass]
    public partial class GravityTrait : MovementTraitResource
    {
        public override void PreProcess(ref MovementContext ctx) { }

        public override void Process(ref MovementContext ctx, ref Vector3 velocity, float delta)
        {
            if (ctx.IsOnFloor)
            {
                // Prevent velocity from accumulating downward while grounded
                if (velocity.Y < 0f)
                    velocity.Y = 0f;
                return;
            }

            velocity += ctx.Gravity * delta;
        }

        public override void PostProcess(ref MovementContext ctx) { }
    }
}
