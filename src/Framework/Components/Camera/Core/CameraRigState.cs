// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Framework.Components.Camera.Core
{
	/// <summary>
	/// The persistent, accumulated working state of a camera rig. This is what traits read and
	/// write every frame - the camera equivalent of the <c>Vector3 velocity</c> threaded through
	/// <c>IMovementTrait.Process</c>, just with more fields because a camera pose is richer than
	/// a single vector.
	///
	/// Fields come in Target/current pairs where a "feel" trait (SmoothingTrait) is expected to
	/// chase the Target value toward the current one over time; a few fields are purely additive
	/// and transient (ExtraDistance, OvershootDistance) - they're expected to decay back to zero
	/// on their own rather than being smoothed toward anything.
	/// </summary>
	public struct CameraRigState
	{
		// ── Rotation ────────────────────────────────────────────────
		/// <summary>Raw accumulated yaw target in degrees, before smoothing.</summary>
		public float TargetYaw;
		/// <summary>Raw accumulated pitch target in degrees, before smoothing.</summary>
		public float TargetPitch;
		/// <summary>Smoothed yaw actually applied to the rig this frame.</summary>
		public float Yaw;
		/// <summary>Smoothed pitch actually applied to the rig this frame.</summary>
		public float Pitch;

		// ── Distance / offset ───────────────────────────────────────
		/// <summary>Desired spring-arm length before smoothing.</summary>
		public float TargetArmLength;
		/// <summary>Smoothed spring-arm length actually applied (excl. lag/overshoot distance).</summary>
		public float ArmLength;
		/// <summary>Transient additive distance from CameraLagTrait. Decays to 0 on its own.</summary>
		public float ExtraDistance;
		/// <summary>
		/// Transient additive distance from CameraOvershootTrait: the camera flies a bit further
		/// out while you're rotating it quickly, then eases back in once you stop. Decays to 0
		/// on its own, independent of ExtraDistance so lag and overshoot never fight each other.
		/// </summary>
		public float OvershootDistance;

		/// <summary>Desired local offset (e.g. over-the-shoulder framing) before smoothing.</summary>
		public Vector3 TargetLocalOffset;
		/// <summary>Smoothed local offset actually applied, in yaw-relative local space.</summary>
		public Vector3 LocalOffset;

		// ── Field of view ───────────────────────────────────────────
		public float TargetFOV;
		public float FOV;

		// ── Follow / pan ────────────────────────────────────────────
		/// <summary>World-space point the rig orbits - usually the followed target's position + eye height.</summary>
		public Vector3 PivotPosition;
		/// <summary>Accumulated free-pan offset from edge-scroll/drag-pan traits, world-space XZ.</summary>
		public Vector3 PanOffset;
	}
}
