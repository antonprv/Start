// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

using System;
using System.Runtime.CompilerServices;

namespace Code.Common.Extensions.Logging
{
	public interface IGameLog
	{
		public void Log(
		LogType logType,
		string message,
		[CallerMemberName] string memberName = "",
		[CallerFilePath] string filePath = "",
		[CallerLineNumber] int lineNumber = 0 );

		public void LogInfo(
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 );

		public void LogWarning(
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 );

		public void LogException(
			Exception exception,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 );

		public void LogError(
			string message,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 );

		public void LogValue<TValue>(
			string propertyName,
			TValue value,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0 );
	}
}