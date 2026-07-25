// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

#if TOOLS
using Godot;
using Framework.Logger;

[Tool]
public partial class Plugin : EditorPlugin
{
	public override void _EnterTree()
	{
		GameLogger.LogInfo( "[Streaming] Plugin loaded successfully" );
	}

	public override void _ExitTree()
	{
		GameLogger.LogInfo( "[Streaming] Plugin unloaded" );
	}
}
#endif
