// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Interfaces;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Doom3
{
	public class Doom3FrictionTrait : IMovementTrait
	{
		public void PreProcess( ref MovementContext ctx ) { }

		public void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
		{
			Vector3 vel = velocity;
			if ( ctx.IsOnFloor )
				vel.Y = 0f;

			float speed = vel.FastLength();
			if ( speed < 1.0f * Core.Doom3Constants.InchesToMeters )
			{
				// remove all movement orthogonal to gravity (lets the player sink)
				if ( FMath.AbsBranchless( velocity.Y ) < 1e-5f )
					velocity = Vector3.Zero;
				else
					velocity = new Vector3( 0f, velocity.Y, 0f );
				return;
			}

			float drop = 0f;
			float k = Core.Doom3Constants.InchesToMeters;

			if ( ctx.IsOnFloor )
			{
				// TODO Phase 2: skip this branch on SURF_SLICK / PMF_TIME_KNOCKBACK
				float stop = Core.Doom3Constants.PM_STOPSPEED * k;
				float control = speed < stop ? stop : speed;
				drop += control * Core.Doom3Constants.PM_FRICTION * delta;
			}
			else
			{
				// air friction is 0.0f in the original - kept explicit for parity
				drop += speed * Core.Doom3Constants.PM_AIRFRICTION * delta;
			}

			float newSpeed = speed - drop;
			if ( newSpeed < 0f )
				newSpeed = 0f;

			velocity *= newSpeed / speed;
		}

		public void PostProcess( ref MovementContext ctx ) { }
	}
}
