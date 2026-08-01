// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Console.Interfaces;

namespace Framework.Console.Commands
{
	public class HelpCommand : IConsoleCommand
	{
		private readonly IDevConsole _console;
		private readonly Dictionary<string, IConsoleCommand> _all;

		public string CommandName => "help";
		public string Description => "Show available commands. Usage: help";

		public HelpCommand( IDevConsole console, Dictionary<string, IConsoleCommand> all )
		{
			_console = console;
			_all = all;
		}

		public void Execute( string[] args )
		{
			_console.AddMessage( "Available commands:" );
			foreach ( var cmd in _all.Values )
				_console.AddMessage( $"  [color=cyan]{cmd.CommandName}[/color] — {cmd.Description}" );
		}
	}
}
