// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Code.Services.Input
{
	public interface IInputService
	{
		Vector2 GetInputVector();
		bool GetJumpState( InputEvent inputEvent );
	}
}