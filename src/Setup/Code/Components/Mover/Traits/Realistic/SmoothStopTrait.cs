// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using FastMath;
using Godot;

namespace Code.Components.Mover.Traits.Realistic
{
	/// <summary>
	/// Lerps horizontal velocity to zero when there is no player input.
	/// Stacks on top of GroundFrictionTrait to produce a snappier stop.
	/// </summary>
	[GlobalClass]
	public partial class SmoothStopTrait : MovementTraitResource
	{
		[Export] public float StopLerpSpeed { get; set; } = 10f;

		public override void PreProcess( ref MovementContext ctx ) { }

		public override void Process( ref MovementContext ctx, ref Vector3 velocity, float delta )
		{
			if ( !ctx.IsOnFloor )
				return;

			if ( ctx.WishDirection.FastLength() > 0.01f )
				return;

			Vector3 horizontal = new Vector3( velocity.X, 0f, velocity.Z );
			horizontal = horizontal.FastLerp( Vector3.Zero, StopLerpSpeed * delta );

			velocity.X = horizontal.X;
			velocity.Z = horizontal.Z;
		}

		public override void PostProcess( ref MovementContext ctx ) { }
	}
}
