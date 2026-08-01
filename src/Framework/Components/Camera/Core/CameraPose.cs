// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Framework.Components.Camera.Core
{
	/// <summary>
	/// A named, code-settable camera pose - what a designer would call a "camera preset" for a
	/// specific character state (walking, crouched/stealth, aiming, ...). Pass one of these to
	/// <c>PoseTrait.SetPose</c> (or <c>CameraComponent.TransitionToPose</c>) to smoothly blend
	/// the rig's shoulder offset/arm length/FOV toward it over a given duration.
	///
	/// Any field left at its default (-1 for ArmLength/FOV) is treated as "don't override" -
	/// only LocalOffset is always applied.
	/// </summary>
	public struct CameraPose
	{
		/// <summary>Yaw-relative local offset (right, up, forward) from the pivot.</summary>
		public Vector3 LocalOffset;

		/// <summary>Overrides the rig's target arm length while this pose is active. -1 = leave as-is.</summary>
		public float ArmLength;

		/// <summary>Overrides the rig's target FOV while this pose is active. -1 = leave as-is.</summary>
		public float FOV;

		public CameraPose( Vector3 localOffset, float armLength = -1f, float fov = -1f )
		{
			LocalOffset = localOffset;
			ArmLength = armLength;
			FOV = fov;
		}
	}
}
