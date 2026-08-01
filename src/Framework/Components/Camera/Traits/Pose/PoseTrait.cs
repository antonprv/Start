// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Framework.FastMath.Godot;
using Framework.FastMath.Godot.Extensions;
using Godot;

namespace Framework.Components.Camera.Traits.Pose
{
	/// <summary>
	/// Owns the rig's local offset (and optionally arm length / FOV) and blends it smoothly
	/// toward whatever pose was last requested via <see cref="SetPose(CameraPose,float)"/>.
	///
	/// This is what lets code say "the character is crouching, ease the camera up and back a
	/// touch" or "the character is walking, ease back to the normal over-the-shoulder framing"
	/// without a visible cut - call <c>CameraComponent.TransitionToPose(...)</c> (which forwards
	/// to this trait) whenever the character's stance changes.
	///
	/// Unlike SmoothingTrait (which continuously chases Target* every frame at a fixed rate),
	/// this trait blends over an explicit duration using a smoothstep easing curve, so a
	/// 0.35s stealth transition always takes 0.35s regardless of distance.
	/// </summary>
	public class PoseTrait : ICameraTrait
	{
		#region Properties

		/// <summary>Pose applied at rest, before any SetPose call - e.g. the default over-the-shoulder framing.</summary>
		public Vector3 DefaultOffset { get; set; } = new Vector3( 0.55f, 0.3f, 0f );
		public float DefaultArmLength { get; set; } = -1f;
		public float DefaultFOV { get; set; } = -1f;

		/// <summary>Blend duration used when SetPose is called without an explicit duration.</summary>
		public float DefaultBlendTime { get; set; } = 0.35f;

		#endregion


		private CameraPose _fromPose;
		private CameraPose _toPose;
		private float _blendDuration = 0.35f;
		private float _blendElapsed;
		private bool _initialized;

		#region Public

		/// <summary>Smoothly blend to a new pose over `duration` seconds. duration &lt;= 0 uses DefaultBlendTime.</summary>
		public void SetPose( CameraPose pose, float duration = -1f )
		{
			_fromPose = _toPose;
			_toPose = pose;
			_blendDuration = duration > 0f ? duration : DefaultBlendTime;
			_blendElapsed = 0f;
		}

		/// <summary>Convenience overload matching CameraComponent.TransitionToPose's parameter list.</summary>
		public void SetPose( Vector3 localOffset, float armLength, float fov, float duration ) =>
			SetPose( new CameraPose( localOffset, armLength, fov ), duration );

		#endregion

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			if ( !_initialized )
			{
				_fromPose = _toPose = new CameraPose( DefaultOffset, DefaultArmLength, DefaultFOV );
				_initialized = true;
			}

			_blendElapsed = FMath.Min( _blendElapsed + delta, _blendDuration );
			float t = _blendDuration > 0f
				? FMath.SmoothStep( FMath.Clamp01( _blendElapsed / _blendDuration ) )
				: 1f;

			state.TargetLocalOffset = _fromPose.LocalOffset.FastLerp( _toPose.LocalOffset, t );

			if ( _toPose.ArmLength >= 0f )
				state.TargetArmLength = FMath.Lerp(
					_fromPose.ArmLength >= 0f ? _fromPose.ArmLength : state.TargetArmLength,
					_toPose.ArmLength,
					t );

			if ( _toPose.FOV >= 0f )
				state.TargetFOV = FMath.Lerp(
					_fromPose.FOV >= 0f ? _fromPose.FOV : state.TargetFOV,
					_toPose.FOV,
					t );
		}

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion
	}
}
