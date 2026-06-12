// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Console.Interfaces;
using Console.Types;
using Game.Code.Components.Mover;

namespace Game.Code.Common.Debug.ConsoleCommands
{
	public class NoclipCommand : IConsoleCommand
	{
		private IDevConsole _console;
		private IMoverComponent _mover;

		public NoclipCommand( IDevConsole console, IMoverComponent moverComponent )
		{
			_console = console;
			_mover = moverComponent;
		}

		public string CommandName => "noclip";

		public string Description => "Toggle: makes player clip through walls and fly.";

		public void Execute( string[] args )
		{
			if ( args.Length != 0 )
			{
				_console.AddMessage( Description, ConsoleMessageType.Warning );
				return;
			}

			_mover.SetNoclip( !_mover.IsNoclip );
			_console.AddMessage( $"Noclip: {( _mover.IsNoclip ? "ON" : "OFF" )}", ConsoleMessageType.Info );
		}
	}
}