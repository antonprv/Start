using Godot;

namespace Code.Services.Input
{
    public interface IInputService
    {
        Vector2 GetInputVector();
        bool GetJumpState(InputEvent inputEvent);
    }
}
