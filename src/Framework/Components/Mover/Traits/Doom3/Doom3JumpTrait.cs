// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core;
using Framework.Components.Mover.Core.Interfaces;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Mover.Traits.Doom3
{
	public partial class Doom3JumpTrait : IMovementTrait
	{
		/// <summary>Mirrors pm_jumpheight - Doom units (inches). Default matches the original's "48".</summary>
		public float MaxJumpHeightInches { get; set; } = 48f;

		public void PreProcess( ref MovementContext ctx ) { }

		public void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
		{
			// "don't jump if we can't stand up" (PMF_DUCKED) is deferred to Phase 2
			// (CheckDuck isn't ported yet), so only the ground + input-pulse gate applies here.
			if ( !ctx.IsOnFloor || !ctx.JumpRequested )
				return;

			float g = FMath.Max( ctx.Gravity.FastLength(), 0.0001f );
			float h = MaxJumpHeightInches * Core.Doom3Constants.InchesToMeters;

			float jumpSpeed = FMath.FastSqrt( 2f * h * g );
			velocity.Y = jumpSpeed;

			ctx.JumpConsumed = true;
		}

		public void PostProcess( ref MovementContext ctx ) { }
	}
}
