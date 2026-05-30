// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Components.Mover.Core;
using Components.Mover.Core.Resources;
using FastMath;
using Godot;

namespace Components.Mover.Traits.Quake
{
    /// <summary>
    /// Quake / Half-Life air strafing.
    ///
    /// Adds acceleration along wish direction only up to AirMaxSpeed in that direction.
    /// Because this cap is applied per-direction (not to total speed), the player
    /// can maintain high total speed from a jump while still steering — the foundation
    /// of bunny hopping and strafe jumping.
    ///
    /// Keep AirMaxSpeed low (0.5–1.0) for authentic Quake feel.
    /// </summary>
    [GlobalClass]
    public partial class QuakeAirStrafeTrait : MovementTraitResource
    {
        public override void PreProcess(ref MovementContext ctx) { }

        public override void Process(ref MovementContext ctx, ref Vector3 velocity, float delta)
        {
            if (ctx.IsOnFloor)
                return;

            Vector3 wishDir = ctx.WishDirection;
            if (wishDir.IsNearlyZero())
                return;

            float wishSpeed = ctx.Profile.AirMaxSpeed;
            float currentSpeed = velocity.FastDot(wishDir);
            float addSpeed = wishSpeed - currentSpeed;

            if (addSpeed <= 0f)
                return;

            float accel = ctx.Profile.AirAcceleration * delta;
            if (accel > addSpeed)
                accel = addSpeed;

            velocity += wishDir * accel;
        }

        public override void PostProcess(ref MovementContext ctx) { }
    }
}
