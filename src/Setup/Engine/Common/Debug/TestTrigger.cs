// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Engine.Common.Physics;

using Godot;
using Setup.Engine.Common.Debug.UI;
using Zenjex;

namespace Engine.Common.Debug
{
	public partial class TestTrigger : TriggerVolume
	{
		private IUIMessage _uIMessage;

		[Inject]
		private void Construct( IUIMessage uIMessage ) => _uIMessage = uIMessage;

		public override void _EnterTree() => DiContainer.Instance.Inject( this );

		public override void Initialize() { }

		protected override void OnBodyEnter( Node3D body ) =>
			_uIMessage.Send( $"{body} entered!" );

		protected override void OnBodyExit( Node3D body ) =>
			_uIMessage.Send( $"{body} exited!" );
	}
}
