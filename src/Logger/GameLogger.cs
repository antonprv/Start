// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Logger
{
	public static class GameLogger
	{
		private static readonly Dictionary<string, List<string>> _logsByAssembly = new Dictionary<string, List<string>>();
		private static readonly object _logsLock = new object();

		[Conditional( "DEBUG" )]
		public static void Log(
			LogType logType,
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
			string assemblyName = Assembly.GetCallingAssembly().GetName().Name;
			WriteLog( assemblyName, logType, message, memberName, filePath, lineNumber );
		}

		[Conditional( "DEBUG" )]
		public static void LogInfo(
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
			string assemblyName = Assembly.GetCallingAssembly().GetName().Name;
			WriteLog( assemblyName, LogType.Info, message, memberName, filePath, lineNumber );
		}

		[Conditional( "DEBUG" )]
		public static void LogWarning(
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
			string assemblyName = Assembly.GetCallingAssembly().GetName().Name;
			WriteLog( assemblyName, LogType.Warning, message, memberName, filePath, lineNumber );
		}

		[Conditional( "DEBUG" )]
		public static void LogException(
			Exception exception,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
			string assemblyName = Assembly.GetCallingAssembly().GetName().Name;
			WriteLog( assemblyName, LogType.Error, exception.ToString(), memberName, filePath, lineNumber );
		}

		[Conditional( "DEBUG" )]
		public static void LogError(
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
			string assemblyName = Assembly.GetCallingAssembly().GetName().Name;
			WriteLog( assemblyName, LogType.Error, message, memberName, filePath, lineNumber );
		}

		[Conditional( "DEBUG" )]
		public static void LogValue<TValue>(
			string propertyName,
			TValue value,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
			string assemblyName = Assembly.GetCallingAssembly().GetName().Name;
			WriteLog( assemblyName, LogType.Info, $"Set {propertyName} to {value}", memberName, filePath, lineNumber );
		}

		#region Save Logs To File

		[Conditional( "DEBUG" )]
		public static void SaveLogsToFile()
		{
			try
			{
				Dictionary<string, List<string>> snapshot;

				lock ( _logsLock )
				{
					snapshot = new Dictionary<string, List<string>>();

					foreach ( var kvp in _logsByAssembly )
					{
						if ( kvp.Value.Count > 0 )
							snapshot[ kvp.Key ] = new List<string>( kvp.Value );

						kvp.Value.Clear();
					}

					_logsByAssembly.Clear();
				}

				if ( snapshot.Count == 0 )
					return;

				string directory = GetLogsDirectory();
				Directory.CreateDirectory( directory );

				foreach ( var kvp in snapshot )
				{
					string fileName = $"{kvp.Key}_log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
					string fullPath = Path.Combine( directory, fileName );
					File.WriteAllLines( fullPath, kvp.Value );

					GD.Print( $"[GameLogger] [{kvp.Key}] Logs saved to: {fullPath} ({kvp.Value.Count} entries)" );
				}
			}
			catch ( Exception exception )
			{
				GD.PushError( $"[GameLogger] Failed to save logs: {exception.Message}" );
			}
		}

		[Conditional( "DEBUG" )]
		public static void ClearLogs()
		{
			lock ( _logsLock )
			{
				_logsByAssembly.Clear();
			}
		}

		#endregion

		#region Private API

		private static string GetLogsDirectory()
		{
			if ( Engine.IsEditorHint() )
			{
				string projectPath = ProjectSettings.GlobalizePath( "res://" ).TrimEnd( '/', '\\' );
				string parentPath = Directory.GetParent( projectPath )?.FullName ?? projectPath;
				return Path.Combine( parentPath, "Logs" );
			}

			string exePath = OS.GetExecutablePath();
			return Path.GetDirectoryName( exePath ) ?? ".";
		}

		[Conditional( "DEBUG" )]
		private static void WriteLog(
			string assemblyName,
			LogType logType,
			string message,
			string memberName,
			string filePath,
			int lineNumber )
		{
			string className =
				Path.GetFileNameWithoutExtension( filePath );

			string formattedMessage =
				$"[{className}.{memberName}:{lineNumber}] {message}";

			lock ( _logsLock )
			{
				if ( !_logsByAssembly.TryGetValue( assemblyName, out var logs ) )
				{
					logs = new List<string>();
					_logsByAssembly[ assemblyName ] = logs;
				}

				logs.Add( $"[{DateTime.Now:HH:mm:ss}] [{logType}] {formattedMessage}" );
			}

			switch ( logType )
			{
			case LogType.Info:
				GD.Print( formattedMessage );
				break;

			case LogType.Warning:
				GD.PushWarning( formattedMessage );
				break;

			case LogType.Error:
				GD.PushError( formattedMessage );
				break;
			}
		}

		#endregion
	}
}