// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

namespace Framework.Console.Types
{
	public class ConsoleMessage
	{
		public ConsoleMessageType Type { get; }
		public string Formatted { get; }

		public ConsoleMessage( string message, ConsoleMessageType type, string marker )
		{
			Type = type;
			Formatted = Format( message, type, marker );
		}

		private static string Format( string message, ConsoleMessageType type, string marker ) =>
			type switch
			{
				ConsoleMessageType.Warning => $"{marker}[color=yellow][WARN][/color] {message}",
				ConsoleMessageType.Error => $"{marker}[color=red][ERR][/color] {message}",
				ConsoleMessageType.Command => $"[color=cyan]> {message}[/color]",
				ConsoleMessageType.Success => $"{marker}[color=green][OK][/color] {message}",
				_ => $"{marker}{message}"
			};
	}
}
