// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;
using Framework.FastMath.Godot;

namespace Framework.Components.Camera.Traits.Distance
{
	/// <summary>
	/// Lets the mouse wheel adjust spring-arm length within [MinArmLength, MaxArmLength] -
	/// the zoom behavior used by BG3's orbit camera and most third-person shoulder cameras.
	/// Seeds the arm length to <see cref="InitialArmLength"/> on its first Process call so a
	/// preset doesn't also need a FixedArmLengthTrait just to set a starting distance.
	/// </summary>
	public class ScrollZoomTrait : ICameraTrait
	{
		#region Properties
		public float InitialArmLength { get; set; } = 8f;
		public float MinArmLength { get; set; } = 2f;
		public float MaxArmLength { get; set; } = 20f;
		public float ZoomStep { get; set; } = 1f;

		#endregion

		private bool _initialized;

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta )
		{
			if ( !_initialized )
			{
				state.TargetArmLength = InitialArmLength;
				_initialized = true;
			}

			if ( ctx.ZoomInput != 0f )
				state.TargetArmLength = FMath.Clamp(
					state.TargetArmLength - ( ctx.ZoomInput * ZoomStep ),
					MinArmLength,
					MaxArmLength );
		}

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion
	}
}
