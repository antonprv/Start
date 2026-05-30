// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Components.Mover.Core.Resources;

namespace Game.Code.Components.Mover.Resources
{
	internal static class MProfileAdapter
	{
		public static MProfile Convert( this MovementProfile profile ) => new MProfile()
		{
			GroundAcceleration = profile.GroundAcceleration,
			MaxSpeed = profile.MaxSpeed,
			GroundFriction = profile.GroundFriction,
			AirAcceleration = profile.AirAcceleration,
			AirMaxSpeed = profile.AirMaxSpeed,
			AirControl = profile.AirControl,
			JumpSpeed = profile.JumpSpeed,
			JumpBufferTime = profile.JumpBufferTime,
			CoyoteTime = profile.CoyoteTime
		};
	}
}
