// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Console.Interfaces;

namespace Framework.Console.Commands
{
	public class ClearCommand : IConsoleCommand
	{
		private readonly IDevConsole _console;

		public string CommandName => "clear";
		public string Description => "Clear console output. Usage: clear";

		public ClearCommand( IDevConsole console ) => _console = console;

		public void Execute( string[] args ) => _console.ClearMessages();
	}
}
