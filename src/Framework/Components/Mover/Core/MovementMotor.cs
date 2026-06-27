// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Interfaces;
using Godot;

namespace Framework.Components.Mover.Core
{
    public sealed class MovementMotor : IMovementMotor
    {
        public Vector3 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        private Vector3 _velocity;
        private List<IMovementTrait> _traits;

        public MovementMotor(List<IMovementTrait> traits)
        {
            _traits = traits;
        }

        /// <inheritdoc cref="IMovementMotor.SetTraits"/>
        public void SetTraits(List<IMovementTrait> traits)
        {
            _traits = traits;
        }

        public void Simulate(float delta, MovementContext ctx)
        {
            // Inject delta into context so PreProcess callbacks can use it
            // without needing an extra parameter on the interface.
            ctx.Delta = delta;

            foreach (var t in _traits)
                t.PreProcess(ref ctx);

            foreach (var t in _traits)
                t.Process(ref ctx, ref _velocity, delta);

            foreach (var t in _traits)
                t.PostProcess(ref ctx);
        }
    }
}
