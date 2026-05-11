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

        public bool GetJumpState(InputEvent inputEvent) =>
          GInput.IsActionJustPressedByEvent(InputNames.Jump, inputEvent);
    }
}
