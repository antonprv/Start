// Created by Anton Piruev in 2026.
// Any direct commercial use of derivative work is strictly prohibited.

using Godot;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Code.Common.Extensions.Logging
{
	public sealed class GameLogger : IGameLog
	{
		public void Log(
			LogType logType,
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
#if DEBUG
			WriteLog(
				logType,
				message,
				memberName,
				filePath,
				lineNumber );
#endif
		}

		public void LogInfo(
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
#if DEBUG
			WriteLog(
				LogType.Info,
				message,
				memberName,
				filePath,
				lineNumber );
#endif
		}

		public void LogWarning(
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
#if DEBUG
			WriteLog(
				LogType.Warning,
				message,
				memberName,
				filePath,
				lineNumber );
#endif
		}

		public void LogException(
			Exception exception,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
#if DEBUG
			WriteLog(
				LogType.Error,
				exception.ToString(),
				memberName,
				filePath,
				lineNumber );
#endif
		}

		public void LogError(
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
#if DEBUG
			WriteLog(
				LogType.Error,
				message,
				memberName,
				filePath,
				lineNumber );
#endif
		}

		public void LogValue<TValue>(
			string propertyName,
			TValue value,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 )
		{
#if DEBUG
			WriteLog(
				LogType.Info,
				$"Set {propertyName} to {value}",
				memberName,
				filePath,
				lineNumber );
#endif
		}

		private static void WriteLog(
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
	}
}