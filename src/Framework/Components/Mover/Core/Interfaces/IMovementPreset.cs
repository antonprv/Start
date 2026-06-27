// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Resources;
using Godot.Collections;

namespace Framework.Components.Mover.Core.Interfaces
{
    public interface IMovementPreset
    {
        Array<MovementTraitResource> Traits { get; set; }

        List<IMovementTrait> BuildTraits();
    }
}