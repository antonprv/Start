// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Physics.Types
{
	public class Vector3Packed
	{
		public Vector3 Value;

		public Vector3Packed( Vector3 value ) =>
			this.Value = value;
	}
}
