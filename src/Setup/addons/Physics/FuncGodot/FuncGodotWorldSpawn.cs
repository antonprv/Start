// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;
using Physics;

namespace Setup.addons.Physics.FuncGodot
{
	[GlobalClass]
	public partial class FuncGodotWorldSpawn : BepuStaticBody3D
	{
		protected override void OnRegister()
		{
			if ( IsValid )
				base.OnRegister();
		}
	}
}
