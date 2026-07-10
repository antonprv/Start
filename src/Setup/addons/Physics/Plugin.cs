// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

#if TOOLS

using Godot;

namespace Physics
{
	[Tool]
	public partial class Plugin : EditorPlugin
	{
		public override void _EnterTree()
		{
			GD.Print( "[Physics] Plugin loaded successfully" );
		}

		public override void _ExitTree()
		{
			GD.Print( "[Physics] Plugin unloaded" );
		}
	}
}

#endif
