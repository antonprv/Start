using Code.Common.FastMath;
using Code.Services.Input;

using Godot;

using ZenjexGodot;

namespace Code.Components
{
    public partial class Mover : CharacterBody3D
    {
        [Export]
        public float moveSpeed = 5f;

        [Inject] private IInputService _inputService;

        private Vector2 _inputDirection;
        private bool _wantsToJump;

        public override void _Ready() => DiContainer.Instance.Inject(this);

        public override void _UnhandledKeyInput(InputEvent @event)
        {
            _inputDirection = _inputService.GetInputVector();
            _wantsToJump = _inputService.GetJumpState(@event);
        }

        public override void _PhysicsProcess(double delta)
        {
            HandleMovement();
            MoveAndSlide();
        }

        private void HandleMovement()
        {
            if (_inputDirection.IsNearlyEqual(Vector2.Zero))
            {
                Velocity = Vector3.Zero;
                return;
            }

            Vector3 moveDirection = new Vector3(_inputDirection.X, 0, _inputDirection.Y).Normalized();
            Velocity = moveDirection * moveSpeed;
        }
    }
}
