// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Framework.FastMath.Godot;
using Godot;

namespace Framework.Components.Camera.Traits.Feel
{
	/// <summary>
	/// Adds extra spring-arm distance proportional to how fast the followed target is moving
	/// horizontally, so the camera visibly "can't keep up" during a sprint - and then eases back
	/// in once the target slows down or stops.
	///
	/// The two rates are intentionally asymmetric: <see cref="BuildSpeed"/> is fast so the lag
	/// shows up almost immediately when the character takes off, while <see cref="RecoverSpeed"/>
	/// is slow so the camera drifts back in gently instead of snapping back the instant movement
	/// stops - which is what actually reads as "lag" rather than just "noisy distance".
	/// </summary>
	public class CameraLagTrait : ICameraTrait
	{
		#region Properties

		/// <summary>Extra meters of distance per m/s of target horizontal speed.</summary>
		public float LagPerSpeed { get; set; } = 0.15f;

		/// <summary>Hard cap on how far the lag can push the camera out.</summary>
		public float MaxLagDistance { get; set; } = 3f;

		/// <summary>How fast (units/sec, used by MoveTowards) the lag distance grows when speeding up.</summary>
		public float BuildSpeed { get; set; } = 8f;

		/// <summary>How fast (units/sec) the lag distance shrinks back to zero once the target slows/stops.</summary>
		public float RecoverSpeed { get; set; } = 2f;

		#endregion

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			Vector3 flatVelocity = ctx.TargetVelocity;
			flatVelocity.Y = 0f;

			float speed = flatVelocity.Length();
			float desired = FMath.Clamp( speed * LagPerSpeed, 0f, MaxLagDistance );

			float rate = desired > state.ExtraDistance ? BuildSpeed : RecoverSpeed;
			state.ExtraDistance = MoveTowardsF( state.ExtraDistance, desired, rate * delta );
		}

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion

		#region Helpers

		private static float MoveTowardsF( float current, float target, float maxDelta )
		{
			float diff = target - current;
			if ( FMath.Abs( diff ) <= maxDelta )
				return target;

			return current + ( FMath.Sign( diff ) * maxDelta );
		}

		#endregion
	}
}
