// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Interfaces;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Custom
{
	public class Doom3GroundAccelTrait : IMovementTrait
	{
		public void PreProcess( ref MovementContext ctx ) { }

		public void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
		{
			if ( !ctx.IsOnFloor )
				return;

			float inputMag = FMath.Min( ctx.WishDirection.FastLength(), 1f );
			if ( inputMag < 0.0001f )
				return;

			Vector3 wishdir = ctx.WishDirection.FastNormalized();
			float wishspeed = ctx.Profile.MaxSpeed * inputMag;

			// q2-style Accelerate() - Physics_Player.cpp lines 123-138.
			float currentspeed = velocity.FastDot( wishdir );
			float addspeed = wishspeed - currentspeed;
			if ( addspeed <= 0f )
				return;

			float accelspeed = Core.Doom3Constants.PM_ACCELERATE * delta * wishspeed;
			if ( accelspeed > addspeed )
				accelspeed = addspeed;

			velocity += accelspeed * wishdir;
		}

		public void PostProcess( ref MovementContext ctx ) { }
	}
}
