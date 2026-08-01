// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Interfaces;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Custom
{
	public class StrafeAirControlTrait : IMovementTrait
	{
		public void PreProcess( ref MovementContext ctx ) { }

		public void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
		{
			if ( ctx.IsOnFloor )
				return;

			Vector3 wishDir = ctx.WishDirection;
			if ( wishDir.IsNearlyZero() )
				return;

			// Cap comes from AirMaxSpeed, not MaxSpeed - set this generously
			// (>= MaxSpeed) for Doom-2016 comfort, or low (0.5-1.0) to fall
			// back to authentic Quake strafe-jump restriction.
			float wishSpeed = ctx.Profile.AirMaxSpeed;

			float currentSpeed = velocity.FastDot( wishDir );
			float addSpeed = wishSpeed - currentSpeed;
			if ( addSpeed <= 0f )
				return;

			// Doom 3 / q2-style curve: proportional to wishspeed rather than
			// a flat ramp. AirAcceleration here plays the role of
			// PM_AIRACCELERATE (default was 1.0 in the original - this trait
			// takes it from the profile instead so it's tunable per-preset).
			float accelSpeed = ctx.Profile.AirAcceleration * delta * wishSpeed;
			if ( accelSpeed > addSpeed )
				accelSpeed = addSpeed;

			velocity += wishDir * accelSpeed;
		}

		public void PostProcess( ref MovementContext ctx ) { }
	}
}
