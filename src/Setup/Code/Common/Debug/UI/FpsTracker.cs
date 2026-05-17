// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Code.Common.Debug.UI
{
	public partial class FpsTracker : Label
	{
		public override void _Process( double delta )
		{
			Text = $"FPS: {Engine.GetFramesPerSecond()}";
		}
	}
}