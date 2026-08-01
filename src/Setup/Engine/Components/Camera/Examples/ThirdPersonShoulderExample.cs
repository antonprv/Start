// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core.Types;
using Godot;

namespace Engine.Components.Camera.Examples
{
	public partial class ThirdPersonShoulderExample : Node
	{
		[Export] private CameraComponent _camera;

		// Same numbers ThirdPersonShoulderPreset uses for its PoseTrait.DefaultOffset - the
		// "walk" pose to blend back to when the player stands up.
		private static readonly Vector3 WalkOffset = new( 0.55f, 0.25f, 0f );
		private static readonly Vector3 CrouchOffset = new( 0.35f, 0.55f, 0f ); // higher and closer

		private bool _crouching;

		public override void _Ready() =>
			_camera.SetCameraMode( CameraPreset.ThirdPersonShoulder );

		public override void _UnhandledInput( InputEvent @event )
		{
			if ( @event is not InputEventKey { Keycode: Key.C, Pressed: true, Echo: false } )
				return;

			_crouching = !_crouching;

			_camera.TransitionToPose(
				_crouching ? CrouchOffset : WalkOffset,
				duration: 0.5f );
		}
	}
}
