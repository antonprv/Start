// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Framework.Components.Camera.Core
{
	/// <summary>
	/// Per-frame, read-mostly snapshot of everything a camera trait might need: what's being
	/// followed, raw input, and delta time. The camera equivalent of <c>MovementContext</c>.
	///
	/// CameraComponent fills this in once per frame before calling <c>ICameraMotor.Simulate</c>.
	/// Traits should treat it as input - the only thing they're supposed to mutate is
	/// <see cref="CameraRigState"/>.
	/// </summary>
	public struct CameraContext
	{
		/// <summary>World-space position of whatever the camera is following (ground/pivot point).</summary>
		public Vector3 TargetPosition;

		/// <summary>
		/// World-space velocity of the followed target, derived by CameraComponent from
		/// frame-to-frame position deltas (lightly smoothed). Used by CameraLagTrait.
		/// </summary>
		public Vector3 TargetVelocity;

		/// <summary>Facing basis of the followed target - useful for shoulder-offset alignment.</summary>
		public Basis TargetBasis;

		/// <summary>Raw look delta this frame (already scaled by sensitivity), X = yaw, Y = pitch.</summary>
		public Vector2 LookInput;

		/// <summary>
		/// Screen-edge / free-pan input this frame, X/Y each in roughly [-1, 1].
		/// Populated from cursor-to-edge distance for RTS/Diablo-style edge scrolling.
		/// </summary>
		public Vector2 PanInput;

		/// <summary>Mouse-wheel delta accumulated this frame (positive = zoom in).</summary>
		public float ZoomInput;

		/// <summary>True while the configured "orbit" button (e.g. RMB) is held - used by drag-to-orbit traits.</summary>
		public bool OrbitHeld;

		/// <summary>Frame delta. Set by CameraMotor before PreProcess, same convention as MovementContext.Delta.</summary>
		public float Delta;
	}
}
