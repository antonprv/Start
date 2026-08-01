// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

namespace Framework.Common.Extensions
{
	public static class DebugExtensions
	{
		public static void DestroyIfNotDebug( this Node node )
		{
			if ( !OS.IsDebugBuild() )
				node.QueueFree();
		}

		public static void DestroyIfNotDebugDeferred( this Node node )
		{
			if ( !OS.IsDebugBuild() )
				node.CallDeferred( nameof( node.QueueFree ) );
		}
	}
}
