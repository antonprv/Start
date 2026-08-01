// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;

namespace Framework.Components.Camera.Traits.Rotation
{
	/// <summary>
	/// Locks the camera to a fixed pitch (and, by default, a fixed yaw) - the classic
	/// Diablo/isometric-ARPG angled top-down look. Optionally allows slow yaw rotation from
	/// input (e.g. Q/E turntable controls) while keeping the pitch pinned.
	/// </summary>
	public class FixedAngleTrait : ICameraTrait
	{
		#region Properties
		public float FixedPitch { get; set; } = -55f;
		public float FixedYaw { get; set; } = 45f;

		/// <summary>If true, ctx.LookInput.X slowly rotates yaw instead of the yaw staying pinned.</summary>
		public bool AllowYawInput { get; set; } = false;

		/// <summary>Degrees/second applied when AllowYawInput is true.</summary>
		public float YawRotateSpeed { get; set; } = 60f;

		#endregion

		private bool _yawInitialized;

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			state.TargetPitch = FixedPitch;

			if ( AllowYawInput )
			{
				if ( !_yawInitialized )
				{
					state.TargetYaw = FixedYaw;
					_yawInitialized = true;
				}

				state.TargetYaw += ctx.LookInput.X * YawRotateSpeed * delta;
			}
			else
			{
				state.TargetYaw = FixedYaw;
			}
		}

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion
	}
}
