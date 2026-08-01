// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Components.Camera.Core.Types;
using Godot;

namespace Engine.Components.Camera.Examples
{
	public partial class EdgeScrollTopDownExample : Node
	{
		[Export] private CameraComponent _camera;

		public override void _Ready() =>
			_camera.SetCameraMode( CameraPreset.EdgeScrollTopDown );
	}
}
