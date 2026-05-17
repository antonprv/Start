// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Components.Mover.Core;
using FastMath;
using Godot;

namespace Components.Mover.Traits.Quake
{
	/// <summary>
	/// Quake-style ground acceleration.
	/// Adds impulses toward wish direction without exceeding MaxSpeed
	/// in that direction — allows building speed via strafing.
	/// </summary>
	[GlobalClass]
	public partial class GroundAccelerationTrait : MovementTraitResource
	{
		public override void PreProcess( ref MovementContext ctx ) { }

		public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
		{
			if ( !ctx.IsOnFloor )
				return;

			Vector3 wishDir = ctx.WishDirection;
			if ( wishDir.IsNearlyZero() )
				return;

			Vector3 wishVel  = wishDir * ctx.Profile.MaxSpeed;
			Vector3 deltaVel = wishVel - velocity;
			deltaVel.Y = 0f;

			float deltaVelLen = deltaVel.FastLength();
			if ( deltaVelLen < 0.001f )
				return;

			float accel = ctx.Profile.GroundAcceleration * delta;
			if ( accel > deltaVelLen )
				accel = deltaVelLen;

			velocity += deltaVel.FastNormalized() * accel;
		}

		public override void PostProcess( ref MovementContext ctx ) { }
	}
}
