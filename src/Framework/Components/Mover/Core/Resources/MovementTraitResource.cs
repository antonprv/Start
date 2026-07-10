// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Interfaces;
using Godot;

namespace Framework.Components.Mover.Core.Resources
{
    /// <summary>
    /// Base class for ScriptableObject-style movement traits.
    ///
    /// Extend this instead of implementing IMovementTrait directly when you want
    /// the trait to be configurable in the Godot inspector and saveable as a .tres file.
    ///
    /// Usage:
    ///   1. Subclass this and mark with [GlobalClass].
    ///   2. Add [Export] properties for any tuning data.
    ///   3. Create a .tres instance in the inspector.
    ///   4. Assign the instance to a MovementPreset resource.
    ///
    /// Example — custom trait with inspector data:
    /// <code>
    ///   [GlobalClass]
    ///   public partial class DashTrait : MovementTraitResource
    ///   {
    ///       [Export] public float DashSpeed    { get; set; } = 20f;
    ///       [Export] public float DashDuration { get; set; } = 0.15f;
    ///       ...
    ///   }
    /// </code>
    /// </summary>
    public abstract partial class MovementTraitResource : Resource, IMovementTrait
    {
        public abstract void PreProcess( ref MovementContext ctx );
        public abstract void Process( ref MovementContext ctx, ref Vector3 velocity, float delta );
        public abstract void PostProcess( ref MovementContext ctx );
    }
}
