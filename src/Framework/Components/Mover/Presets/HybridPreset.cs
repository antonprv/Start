// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Interfaces;
using Framework.Components.Mover.Core.Resources;
using Framework.Components.Mover.Traits.Common;
using Framework.Components.Mover.Traits.Hybrid;

namespace Framework.Components.Mover.Presets
{
    /// <summary>
    /// Hybrid / Arcade preset.
    ///
    /// Characteristics:
    ///   • Ground: lerp directly toward target velocity — instant feel, no friction needed
    ///   • Air: symmetric lerp control, responsive
    ///   • No friction trait needed — lerp handles both acceleration and deceleration
    ///
    /// Good for: 3D platformers, arcade shooters, anything that should feel
    /// responsive and direct without Quake's speed-preservation.
    /// </summary>
    public static class HybridPreset
    {
        public static List<IMovementTrait> Build() => new()
        {
            new GravityTrait(),
            new JumpTrait(),
            new HybridGroundTrait(),
            new HybridAirControlTrait()
        };

        public static MovementProfile DefaultProfile() => new()
        {
            GroundAcceleration = 14f,   // lerp speed on ground
            MaxSpeed = 6f,
            GroundFriction = 0f,    // unused in this preset
            AirAcceleration = 0f,    // unused in this preset
            AirMaxSpeed = 5f,
            AirControl = 5f,    // lerp speed in air
            JumpSpeed = 5.5f,
            JumpBufferTime = 0.15f,
            CoyoteTime = 0.15f
        };
    }
}
