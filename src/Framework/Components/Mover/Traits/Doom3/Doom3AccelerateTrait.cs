// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Interfaces;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Doom3
{
	public class Doom3AccelerateTrait : IMovementTrait
	{
		public void PreProcess( ref MovementContext ctx ) { }

		public void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
		{
			float inputMag = FMath.Min( ctx.WishDirection.FastLength(), 1f );
			if ( inputMag < 0.0001f )
				return;

			Vector3 wishdir = ctx.WishDirection.FastNormalized();
			float wishspeed = ctx.Profile.MaxSpeed * inputMag;

			float accel = ctx.IsOnFloor
				? Core.Doom3Constants.PM_ACCELERATE
				: Core.Doom3Constants.PM_AIRACCELERATE;

			Accelerate( wishdir, wishspeed, accel, ref velocity, delta );
		}

		/// <summary>Direct port of idPhysics_Player::Accelerate — q2-style, lines 123-138.</summary>
		private void Accelerate( Vector3 wishdir, float wishspeed, float accel, ref Vector3 velocity, float frametime )
		{
			float currentspeed = velocity.FastDot( wishdir );
			float addspeed = wishspeed - currentspeed;
			if ( addspeed <= 0f )
				return;

			float accelspeed = accel * frametime * wishspeed;
			if ( accelspeed > addspeed )
				accelspeed = addspeed;

			velocity += accelspeed * wishdir;
		}

		public void PostProcess( ref MovementContext ctx ) { }
	}
}
