// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Components.Mover.Core;
using Components.Mover.Traits.Common;
using Components.Mover.Traits.Realistic;

namespace Components.Mover.Presets
{
	/// <summary>
	/// Realistic / Source-engine inspired preset.
	///
	/// Characteristics:
	///   • Ground: friction-based deceleration + impulse acceleration (CS:GO feel)
	///   • Stop: smooth lerp to zero when no input
	///   • Air: limited directional nudging, speed capped
	///
	/// Note: RealisticGroundAccelTrait is included because without it the character
	/// cannot accelerate on the ground — GroundFriction alone only decelerates.
	/// </summary>
	public static class RealisticPreset
	{
		public static List<IMovementTrait> Build() => new()
		{
			new GravityTrait(),
			new JumpTrait(),
			new GroundFrictionTrait(),
			new RealisticGroundAccelTrait(),
			new SmoothStopTrait(),
			new ClampedAirControlTrait()
		};

		public static MovementProfile DefaultProfile() => new()
		{
			GroundAcceleration = 20f,
			MaxSpeed           = 5f,
			GroundFriction     = 10f,
			AirAcceleration    = 4f,
			AirMaxSpeed        = 4f,
			AirControl         = 1.5f,
			JumpSpeed          = 5f,
			JumpBufferTime     = 0.10f,
			CoyoteTime         = 0.10f
		};
	}
}
