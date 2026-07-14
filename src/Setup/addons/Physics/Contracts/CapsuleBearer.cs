// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Physics.Contracts
{
	public partial class CapsuleBearer : CollisionShape3D
	{
		public CapsuleShape3D Capsule => Shape as CapsuleShape3D;

		public override string[] _GetConfigurationWarnings() =>
			System.Array.Empty<string>();
	}
}
