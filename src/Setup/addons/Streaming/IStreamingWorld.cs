// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Streaming;

namespace Streaming
{
	/// <summary>
	/// The Godot-facing streaming service. Wraps a <see cref="StreamingWorld"/>
	/// (exposed directly via <see cref="Core"/> for components to call) plus the one thing Core
	/// deliberately knows nothing about: which camera/viewer position drives distance-based
	/// residency this frame.
	/// </summary>
	public interface IStreamingWorld
	{
		/// <summary>The engine-agnostic streaming facade. Its API only uses plain data types -
		/// never a Godot type, never a raw RenderingDevice/RID.</summary>
		StreamingWorld Core { get; }
	}
}
