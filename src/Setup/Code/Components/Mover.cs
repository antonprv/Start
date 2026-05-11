using Code.Common.FastMath;
using Code.Services.Input;

using Godot;
using System;
using ZenjexGodot;

namespace Code.Components
{
    public partial class Mover : CharacterBody3D
    {
        [ExportGroup("Movement")]
        [Export] private float _moveSpeed = 5f;
        [Export] private float _rotationSpeed = 6f;

        [ExportGroup("References")]
        [Export] private Node3D _playerModel;

        [Inject] private IInputService _inputService;

        private Vector3 _moveDirection;
        private Vector2 _inputDirection;
        private bool _wantsToJump;

        public override void _Ready() => DiContainer.Instance.Inject(this);

        public override void _UnhandledKeyInput(InputEvent @event)
        {
            BindInputs(@event);
            HandleInputs();
        }

        public override void _PhysicsProcess(double delta)
        {
            HandleMovement();
            HandleRotation();
            MoveAndSlide();
        }

        private void BindInputs(InputEvent @event)
        {
            _inputDirection = _inputService.GetInputVector();
            _wantsToJump = _inputService.GetJumpState(@event);
        }

        private void HandleInputs() => 
            _moveDirection = new Vector3(_inputDirection.X, 0, _inputDirection.Y).Normalized();

        private void HandleMovement()
        {
            if (_moveDirection.IsNearlyEqual(Vector3.Zero))
            {
                Velocity = Vector3.Zero;
                return;
            }

            Velocity = _moveDirection * _moveSpeed;
        }

        private void HandleRotation()
        {
            // stub
        }
    }
}
