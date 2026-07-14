// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Setup.Engine.Common.Debug
{
	public partial class DestroyInGame : Node
	{
		public override void _Ready() => QueueFree();
	}
}
