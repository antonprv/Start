// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Console.Common;
using Framework.Console.Interfaces;
using Framework.Console.Types;
using Godot;

namespace Framework.Console.Commands
{
	public class StatFpsCommand : IConsoleCommand
	{
		private readonly IDevConsole _console;
		private readonly Node _fpsNode;
		private bool _fpsState;

		private string _usage = "Usage: stat_fps";

		public string CommandName => "stat_fps";
		public string Description => $"Print current FPS. {_usage}";

		public StatFpsCommand( IDevConsole console, Node fpsNode )
		{
			_console = console;
			_fpsNode = fpsNode;
		}

		public void Execute( string[] args )
		{
			if ( args.Length > 0 )
			{
				_console.AddMessage( $"Command error. {_usage}", ConsoleMessageType.Warning );
				return;
			}

			_fpsNode.SetEnabled( _fpsState );
			_fpsState = !_fpsState;
			_console.AddMessage( $"Set fps tracking to {_fpsNode}" );
		}
	}
}
