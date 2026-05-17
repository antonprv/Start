// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Components.Mover.Core
{
	public struct MovementContext
	{
		public Vector3 WishDirection;

		/// <summary>Set true the frame a jump button is pressed.</summary>
		public bool JumpRequested;

		/// <summary>Set true by JumpTrait the frame a jump actually fires.
		/// Read by animation / sound layers.</summary>
		public bool JumpConsumed;

		/// <summary>Current physics delta. Set by MovementMotor before PreProcess
		/// so that PreProcess callbacks can use it without a separate parameter.</summary>
		public float Delta;

		public bool    IsOnFloor;
		public Vector3 Gravity;

		public MovementProfile Profile;
	}
}
