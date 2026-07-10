// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Common.Extensions;

using Godot;

using GEngine = Godot.Engine;

namespace Engine.Common.Debug.UI
{
	public partial class FpsTracker : Label
	{
		public override void _EnterTree() =>
			this.DestroyIfNotDebug();

		public override void _Process( double delta ) =>
			Text = $"FPS: {GEngine.GetFramesPerSecond()}";
	}
}