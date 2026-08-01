// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Framework.FastMath.Godot;

namespace Framework.Components.Camera.Traits.Feel
{
	/// <summary>
	/// The "Metro Gravity" camera overshoot (mrkogamedev): whip the camera around fast and it
	/// flies a bit further out than its normal distance, then eases back in once you stop
	/// spinning - inertia on the arm length itself, not a rotation wobble.
	///
	/// Watches how many degrees TargetYaw/TargetPitch moved this frame (a proxy for how hard
	/// you're currently rotating the camera) and pushes <see cref="CameraRigState.OvershootDistance"/>
	/// out toward a distance proportional to that. Like CameraLagTrait, the two rates are
	/// asymmetric on purpose: <see cref="BuildSpeed"/> is fast so the fly-out is felt almost
	/// immediately on a hard flick, <see cref="RecoverSpeed"/> is slow so it settles back to the
	/// normal distance instead of snapping - that's what actually reads as inertia rather than
	/// just noise.
	/// </summary>
	public class CameraOvershootTrait : ICameraTrait
	{
		#region Properties

		/// <summary>Extra meters of distance per degree of yaw/pitch rotated this frame.</summary>
		public float OvershootPerDegree { get; set; } = 0.03f;

		/// <summary>Hard cap on how far a fast spin can push the camera out.</summary>
		public float MaxOvershootDistance { get; set; } = 1.5f;

		/// <summary>How fast (units/sec, used by MoveTowards) the overshoot distance grows on a fast turn.</summary>
		public float BuildSpeed { get; set; } = 10f;

		/// <summary>How fast (units/sec) the overshoot distance eases back to 0 once you stop turning.</summary>
		public float RecoverSpeed { get; set; } = 3f;

		#endregion

		private float _lastYaw;
		private float _lastPitch;
		private bool _initialized;

		#region ICameraTrait
		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			if ( !_initialized )
			{
				_lastYaw = state.TargetYaw;
				_lastPitch = state.TargetPitch;
				_initialized = true;
			}

			float deltaYaw = FMath.DeltaAngle( _lastYaw, state.TargetYaw );
			float deltaPitch = state.TargetPitch - _lastPitch;
			float angularDelta = FMath.FastSqrt( ( deltaYaw * deltaYaw ) + ( deltaPitch * deltaPitch ) );

			float desired = FMath.Clamp( angularDelta * OvershootPerDegree, 0f, MaxOvershootDistance );
			float rate = desired > state.OvershootDistance ? BuildSpeed : RecoverSpeed;
			state.OvershootDistance = MoveTowardsF( state.OvershootDistance, desired, rate * delta );

			_lastYaw = state.TargetYaw;
			_lastPitch = state.TargetPitch;
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
