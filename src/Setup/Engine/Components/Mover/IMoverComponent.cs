// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Engine.Components.Mover.Resources;
using Framework.Components.Mover.Core;
using Physics;
using Physics.Types;

namespace Engine.Components.Mover
{
	public interface IMoverComponent
	{
		MovementMode CurrentMode { get; }
		MovementMode InitialMode { get; set; }
		bool IsNoclip { get; }
		MProfile Profile { get; set; }
		CollisionLayer Layer { get; set; }
		CollisionLayer Mask { get; set; }
		Vector3Packed Velocity { get; set; }
		BoolPacked IsOnFloor { get; }

		void SetMovementMode( MovementMode mode );
		void SetNoclip( bool enabled );
	}
}