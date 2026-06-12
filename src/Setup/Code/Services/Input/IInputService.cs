// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Game.Code.Services.Input
{
	public interface IInputService
	{
		bool CapturePlayerInput { get; set; }
		Vector2 GetCameraVector();
		Vector2 GetInputVector();
		bool IsJumpPressed();
		bool IsConsolePressed();
	}
}