// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Components.Mover.Core.Interfaces
{
    public interface IMovementTrait
    {
        /// <summary>
        /// Runs before velocity integration.
        /// Use for timers, state tracking, input buffering.
        /// ctx.Delta is guaranteed to be set here.
        /// </summary>
        void PreProcess(ref MovementContext ctx);

        /// <summary>Integrates velocity.</summary>
        void Process(ref MovementContext ctx, ref Vector3 velocity, float delta);

        /// <summary>Runs after all traits have processed. Use for clamping, events.</summary>
        void PostProcess(ref MovementContext ctx);
    }
}
