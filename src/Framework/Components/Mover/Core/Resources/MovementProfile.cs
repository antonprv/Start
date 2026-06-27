// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Mover.Core.Interfaces;
using Godot;

namespace Framework.Components.Mover.Core.Resources
{
    /// <summary>
    /// ScriptableObject-style data asset for movement tuning.
    /// Create via Godot inspector: New Resource → MovementProfile.
    /// Save as .tres and reuse across scenes.
    /// </summary>
    [GlobalClass]
    public partial class MovementProfile : Resource, IMovementProfile
    {
        // ── Ground ──────────────────────────────────────────────────
        [Export] public float GroundAcceleration { get; set; } = 20f;
        [Export] public float MaxSpeed { get; set; } = 7f;
        [Export] public float GroundFriction { get; set; } = 12f;

        // ── Air ─────────────────────────────────────────────────────
        /// <summary>How fast velocity can grow in air per second.</summary>
        [Export] public float AirAcceleration { get; set; } = 8f;

        /// <summary>
        /// Cap for air speed projection onto wish dir.
        /// Keep low (0.5–1.0) for Quake-style strafe jumping;
        /// set equal to MaxSpeed for full air control.
        /// </summary>
        [Export] public float AirMaxSpeed { get; set; } = 5f;

        /// <summary>Lerp factor used by Hybrid/Realistic air traits.</summary>
        [Export] public float AirControl { get; set; } = 3f;

        // ── Jump ─────────────────────────────────────────────────────
        [Export] public float JumpSpeed { get; set; } = 6f;

        /// <summary>Time window after pressing jump before landing where jump still fires.</summary>
        [Export] public float JumpBufferTime { get; set; } = 0.15f;

        /// <summary>Time after leaving ground during which a jump can still fire.</summary>
        [Export] public float CoyoteTime { get; set; } = 0.12f;
    }
}
