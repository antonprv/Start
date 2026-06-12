// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;
using GInput = Godot.Input;

namespace Game.Code.Services.Input
{
	public partial class InputService : Node, IInputService
	{
		public bool CapturePlayerInput { get; set; } = true;

		private Vector2 _mouseDelta;

		public override void _Input( InputEvent inputEvent )
		{
			if ( !CapturePlayerInput )
			{
				GInput.MouseMode = GInput.MouseModeEnum.Confined;
				return;
			}

			GInput.MouseMode = GInput.MouseModeEnum.Captured;
			if ( inputEvent is InputEventMouseMotion eventMouseMotion )
				_mouseDelta += eventMouseMotion.Relative;
		}

		public Vector2 GetCameraVector()
		{
			if ( !CapturePlayerInput )
				return Vector2.Zero;

			Vector2 delta = _mouseDelta;
			_mouseDelta = Vector2.Zero;
			return delta;
		}

		public Vector2 GetInputVector()
		{
			if ( !CapturePlayerInput )
				return Vector2.Zero;

			return GInput.GetVector(
			InputNames.MoveLeft,
			InputNames.MoveRight,
			InputNames.MoveForward,
			InputNames.MoveBack
			);
		}

		public bool IsJumpPressed()
		{
			if ( !CapturePlayerInput )
				return false;

			return GInput.IsActionJustPressed( InputNames.Jump );
		}

		public bool IsConsolePressed() =>
			GInput.IsActionJustPressed( InputNames.CallConsole );
	}
}