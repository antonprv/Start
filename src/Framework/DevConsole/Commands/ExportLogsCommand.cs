// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Framework.Console.Interfaces;
using Framework.Console.Types;
using Godot;
using System.Text.RegularExpressions;

namespace Framework.Console.Commands
{
	public class ExportLogsCommand : IConsoleCommand
	{
		private readonly IDevConsole _console;

		public string CommandName => "export_logs";
		public string Description => "Save console output to file. Usage: export_logs";

		public ExportLogsCommand( IDevConsole console ) => _console = console;

		public void Execute( string[] args )
		{
			try
			{
				string[] messages = _console.GetMessages();

				if ( messages.Length == 0 )
				{
					_console.AddMessage( "No messages to export.", ConsoleMessageType.Warning );
					return;
				}

				string[] plain = Array.ConvertAll( messages, StripBBCode );

				string timestamp = DateTime.Now.ToString( "yyyy-MM-dd_HH-mm-ss" );

				string dir = GetDirectory();
				Directory.CreateDirectory( dir );

				string path = Path.Combine( dir, $"console_{timestamp}.txt" );
				File.WriteAllLines( path, plain );

				_console.AddMessage( $"Exported to: {path}", ConsoleMessageType.Success );
			}
			catch ( Exception exception )
			{
				_console.AddMessage( $"Failed to export logs: {exception.Message}", ConsoleMessageType.Error );
			}
		}

		private static string GetDirectory()
		{
			string directory;

			if ( OS.HasFeature( "editor" ) )
			{
				string projectPath = ProjectSettings.GlobalizePath( "res://" )
					.Replace( '/', '\\' )
					.TrimEnd( '\\' );

				string parentPath = Directory.GetParent( projectPath )?.FullName ?? projectPath;
				directory = Path.Combine( parentPath, "Logs" );
			}
			else
			{
				string exePath = OS.GetExecutablePath();
				string exeDirectory = Path.GetDirectoryName( exePath ) ?? ".";
				directory = Path.Combine( exeDirectory, "Logs" );
			}

			return directory;
		}

		private static string StripBBCode( string s ) =>
			Regex.Replace( s, @"\[.*?\]", "" );
	}
}