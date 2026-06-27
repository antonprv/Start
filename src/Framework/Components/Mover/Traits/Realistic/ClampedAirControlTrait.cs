// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Resources;
using Godot;
using Framework.FastMath;

namespace Framework.Components.Mover.Traits.Realistic
{
    /// <summary>
    /// Limited air control via lerp.
    /// Lets the player nudge direction in air, but slowly — realistic feel.
    /// Tune AirControl on the profile: low (1–2) = sluggish, high (5+) = responsive.
    /// </summary>
    [GlobalClass]
    public partial class ClampedAirControlTrait : MovementTraitResource
    {
        public override void PreProcess(ref MovementContext ctx) { }

        public override void Process(ref MovementContext ctx, ref Vector3 velocity, float delta)
        {
            if (ctx.IsOnFloor)
                return;

            Vector3 wishDir = ctx.WishDirection;
            if (wishDir.IsNearlyZero())
                return;

            Vector3 horizontal = new Vector3(velocity.X, 0f, velocity.Z);
            Vector3 target = wishDir * ctx.Profile.AirMaxSpeed;

            horizontal = horizontal.FastLerp(target, ctx.Profile.AirControl * delta);

            velocity.X = horizontal.X;
            velocity.Z = horizontal.Z;
        }

        public override void PostProcess(ref MovementContext ctx) { }
    }
}
