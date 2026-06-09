// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using Console.Interfaces;
using Godot;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Logger
{
    public static class GameLogger
    {
        private static readonly List<string> _logs = new List<string>();
        private static readonly object _logsLock = new object();
        private static IDevConsole _devConsole;

        public static void Initialize( IDevConsole console )
        {
            _devConsole = console;
        }

        [Conditional( "DEBUG" )]
        public static void Log(
            LogType logType,
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0 )
        {
            WriteLog( logType, message, memberName, filePath, lineNumber );
        }

        [Conditional( "DEBUG" )]
        public static void LogInfo(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0 )
        {
            WriteLog( LogType.Info, message, memberName, filePath, lineNumber );
        }

        [Conditional( "DEBUG" )]
        public static void LogWarning(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0 )
        {
            WriteLog( LogType.Warning, message, memberName, filePath, lineNumber );
        }

        [Conditional( "DEBUG" )]
        public static void LogException(
            Exception exception,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0 )
        {
            WriteLog( LogType.Error, exception.ToString(), memberName, filePath, lineNumber );
        }

        [Conditional( "DEBUG" )]
        public static void LogError(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0 )
        {
            WriteLog( LogType.Error, message, memberName, filePath, lineNumber );
        }

        [Conditional( "DEBUG" )]
        public static void LogValue<TValue>(
            string propertyName,
            TValue value,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0 )
        {
            WriteLog( LogType.Info, $"Set {propertyName} to {value}", memberName, filePath, lineNumber );
        }

        #region Save Logs To File

        [Conditional( "DEBUG" )]
        public static void SaveLogsToFile()
        {
            try
            {
                List<string> snapshot;

                lock ( _logsLock )
                {
                    snapshot = new List<string>( _logs );
                    _logs.Clear();
                }

                if ( snapshot.Count == 0 )
                    return;

                string directory = GetLogsDirectory();
                Directory.CreateDirectory( directory );

                string fileName = $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                string fullPath = Path.Combine( directory, fileName );
                File.WriteAllLines( fullPath, snapshot );

                GD.Print( $"[GameLogger] Logs saved to: {fullPath} ({snapshot.Count} entries)" );
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
                _logs.Clear();
            }
        }

        #endregion

        public static string GetLogsDirectory()
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

        #region Private API

        [Conditional( "DEBUG" )]
        private static void WriteLog(
            LogType logType,
            string message,
            string memberName,
            string filePath,
            int lineNumber )
        {
            string className = Path.GetFileNameWithoutExtension( filePath );
            string formattedMessage = $"[{className}.{memberName}:{lineNumber}] {message}";

            lock ( _logsLock )
            {
                _logs.Add( $"[{DateTime.UtcNow:HH:mm:ss}] [{logType}] {formattedMessage}" );
            }

            _devConsole?.AddMessage( formattedMessage );

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