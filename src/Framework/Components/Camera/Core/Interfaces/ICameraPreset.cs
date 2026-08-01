// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Framework.Components.Camera.Core.Interfaces
{
	public interface ICameraPreset
	{
		List<ICameraTrait> Build();
	}
}
