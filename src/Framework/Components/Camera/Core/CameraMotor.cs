// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core.Interfaces;

namespace Framework.Components.Camera.Core
{
	public sealed class CameraMotor : ICameraMotor
	{
		public CameraRigState State
		{
			get => _state;
			set => _state = value;
		}

		private CameraRigState _state;
		private List<ICameraTrait> _traits;

		public CameraMotor( List<ICameraTrait> traits, CameraRigState initialState = default )
		{
			_traits = traits;
			_state = initialState;
		}

		/// <inheritdoc cref="ICameraMotor.SetTraits"/>
		public void SetTraits( List<ICameraTrait> traits ) => _traits = traits;

		public void Simulate( float delta, CameraContext ctx )
		{
			// Same convention as MovementMotor: inject delta into the context so PreProcess
			// callbacks can use it without a separate parameter.
			ctx.Delta = delta;

			foreach ( var t in _traits )
				t.PreProcess( ref ctx );

			foreach ( var t in _traits )
				t.Process( ref ctx, ref _state, delta );

			foreach ( var t in _traits )
				t.PostProcess( ref ctx );
		}
	}
}
