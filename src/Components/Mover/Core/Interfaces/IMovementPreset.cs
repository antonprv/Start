// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Components.Mover.Core.Resources;
using Godot.Collections;

namespace Components.Mover.Core.Interfaces
{
    public interface IMovementPreset
    {
        Array<MovementTraitResource> Traits { get; set; }

        List<IMovementTrait> BuildTraits();
    }
}