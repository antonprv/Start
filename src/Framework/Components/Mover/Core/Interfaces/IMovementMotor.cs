// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Framework.Components.Mover.Core.Interfaces
{
	public interface IMovementMotor
	{
		Vector3 Velocity { get; set; }

		void Simulate( float delta, MovementContext context );

		void SetTraits( List<IMovementTrait> traits );
	}
}
