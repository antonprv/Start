// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core.Types;
using Godot;

namespace Engine.Components.Camera.Examples
{
	public partial class GeneralThirdPersonToFirstPersonSwitchExample : Node
	{
		[Export] private CameraComponent _camera;

		public override void _Ready() =>
			_camera.SetCameraMode( CameraPreset.GeneralThirdPerson );

		public override void _UnhandledInput( InputEvent @event )
		{
			if ( @event is not InputEventKey { Keycode: Key.V, Pressed: true, Echo: false } )
				return;

			bool isFirstPerson = _camera.CurrentMode == CameraPreset.FirstPerson;
			_camera.SetCameraMode( isFirstPerson ? CameraPreset.GeneralThirdPerson : CameraPreset.FirstPerson );
		}
	}
}
