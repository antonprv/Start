// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Engine.Components.Mover.Resources;
using Framework.Components.Mover.Core;

namespace Engine.Components.Mover
{
	public interface IMoverComponent
	{
		MovementMode CurrentMode { get; }
		MovementMode InitialMode { get; set; }
		bool IsNoclip { get; }
		MProfile Profile { get; set; }

		void SetMovementMode( MovementMode mode );
		void SetNoclip( bool enabled );
	}
}