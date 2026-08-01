// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core;
using Framework.Components.Camera.Core.Interfaces;

namespace Framework.Components.Camera.Traits.Distance
{
	public class FixedArmLengthTrait : ICameraTrait
	{
		public float ArmLength { get; set; } = 5f;

		#region ICameraTrait

		public void PreProcess( ref CameraContext ctx ) { }

		public void Process( ref CameraContext ctx, ref CameraRigState state, float delta ) =>
			state.TargetArmLength = ArmLength;

		public void PostProcess( ref CameraContext ctx ) { }

		#endregion
	}
}
