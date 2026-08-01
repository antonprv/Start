// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Camera.Traits.Rotation
{
	/// <summary>
	/// The Souls-like "camera settles in behind you" behavior: if the player hasn't touched the
	/// look input for <see cref="IdleDelay"/> seconds (and, optionally, is currently moving),
	/// yaw slowly rotates to match the followed target's facing direction - exactly what Elden
	/// Ring's camera does on a long sprint with no camera input. It never touches pitch, and it
	/// backs off the instant the player looks around again.
	/// </summary>
	public class AutoRecenterTrait : ICameraTrait
	{
		#region Properties

		/// <summary>Seconds of no look input before recentering kicks in.</summary>
		public float IdleDelay { get; set; } = 1.2f;

		/// <summary>Degrees/second the yaw is allowed to drift toward the target's facing.</summary>
		public float RecenterSpeed { get; set; } = 45f;

		/// <summary>If true, only recenters while the followed target is actually moving.</summary>
		public bool OnlyWhileMoving { get; set; } = true;

		/// <summary>Horizontal speed (m/s) above which the target counts as "moving".</summary>
		public float MinSpeedToRecenter { get; set; } = 0.5f;

		#endregion

		private float _idleTimer;

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			if ( ctx.LookInput.LengthSq() > 0.0001f || ctx.OrbitHeld )
			{
				_idleTimer = 0f;
				return;
			}

			_idleTimer += delta;
			if ( _idleTimer < IdleDelay )
				return;

			if ( OnlyWhileMoving )
			{
				Vector3 flatVelocity = ctx.TargetVelocity;
				flatVelocity.Y = 0f;

				if ( flatVelocity.Length() < MinSpeedToRecenter )
					return;
			}

			// Yxz matches the order CameraComponent uses to build its own rotation, so this
			// stays consistent with however the rig itself interprets yaw.
			float desiredYaw = ctx.TargetBasis.GetEuler( EulerOrder.Yxz ).Y * FMath.Rad2Deg;

			float maxStep = RecenterSpeed * delta;
			float signedDelta = FMath.Clamp( FMath.DeltaAngle( state.TargetYaw, desiredYaw ), -maxStep, maxStep );

			state.TargetYaw += signedDelta;
		}

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion
	}
}
