// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Console.Types;

namespace Framework.Console.Interfaces
{
	public interface IDevConsole
	{
		bool IsOpen { get; }
		string Marker { get; }

		void Toggle();
		void ExecuteCommand( string commandLine );
		void RegisterCommand( IConsoleCommand command );
		void AddMessage( string message, ConsoleMessageType type = ConsoleMessageType.Info );

		string[] GetMessages();
		void ClearMessages();
		void SetFilter( ConsoleMessageType filter );
		ConsoleMessageType GetFilter();
	}
}
