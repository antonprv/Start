namespace Components.Mover.Core.Interfaces
{
    public interface IMovementProfile
    {
        // ── Ground ──────────────────────────────────────────────────
        public float GroundAcceleration { get; set; }
        public float MaxSpeed { get; set; }
        public float GroundFriction { get; set; }

        // ── Air ─────────────────────────────────────────────────────
        public float AirAcceleration { get; set; }
        public float AirMaxSpeed { get; set; }
        public float AirControl { get; set; }

        // ── Jump ─────────────────────────────────────────────────────
        public float JumpSpeed { get; set; }
        public float JumpBufferTime { get; set; }
        public float CoyoteTime { get; set; }
    }
}
