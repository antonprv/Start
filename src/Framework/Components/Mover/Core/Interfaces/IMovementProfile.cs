namespace Framework.Components.Mover.Core.Interfaces
{
	public interface IMovementProfile
	{
		// -- Ground --------------------------------------------------
		float GroundAcceleration { get; set; }
		float MaxSpeed { get; set; }
		float GroundFriction { get; set; }

		// -- Air -----------------------------------------------------
		float AirAcceleration { get; set; }
		float AirMaxSpeed { get; set; }
		float AirControl { get; set; }

		// -- Jump -----------------------------------------------------
		float JumpHeight { get; set; }
		float JumpBufferTime { get; set; }
		float CoyoteTime { get; set; }
	}
}
