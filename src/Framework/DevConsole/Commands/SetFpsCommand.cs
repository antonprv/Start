// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Console.Interfaces;
using Framework.Console.Types;
using Godot;

namespace Framework.Console.Commands
{
	public class SetFpsCommand : IConsoleCommand
	{
		private readonly IDevConsole _console;

		public string CommandName => "set_fps";
		public string Description => "Set target FPS (0 = uncapped). Usage: set_fps <value>";

		public SetFpsCommand( IDevConsole console ) => _console = console;

		public void Execute( string[] args )
		{
			if ( args.Length < 1 )
			{
				_console.AddMessage( Description, ConsoleMessageType.Warning );
				return;
			}

			if ( int.TryParse( args[ 0 ], out int fps ) )
			{
				Engine.MaxFps = fps;
				_console.AddMessage( $"FPS cap: {( fps == 0 ? "uncapped" : fps.ToString() )}", ConsoleMessageType.Success );
			}
			else
				_console.AddMessage( $"Invalid value: '{args[ 0 ]}'", ConsoleMessageType.Error );
		}
	}
}
