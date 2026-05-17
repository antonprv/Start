// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

using GInput = Godot.Input;

namespace Code.Services.Input
{
	public partial class InputService : GodotObject, IInputService
	{
		public Vector2 GetInputVector() =>
		  GInput.GetVector(
			InputNames.MoveLeft,
			InputNames.MoveRight,
			InputNames.MoveForward,
			InputNames.MoveBack
			);
		
		public bool IsJumpPressed() =>
			GInput.IsActionJustPressed( InputNames.Jump );
	}
}