using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace net.vieapps.Components.Utility
{
	public static class Logger
	{
		static ILoggerFactory LoggerFactory { get; set; }

		/// <summary>
		/// Assigns a logger factory
		/// </summary>
		/// <param name="loggerFactory"></param>
		public static void AssignLoggerFactory(ILoggerFactory loggerFactory) => Logger.LoggerFactory = Logger.LoggerFactory ?? loggerFactory;

		/// <summary>
		/// Gets a logger factory
		/// </summary>
		/// <returns></returns>
		public static ILoggerFactory GetLoggerFactory() => Logger.LoggerFactory ?? new NullLoggerFactory();

		/// <summary>
		/// Creates a logger
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILogger CreateLogger(Type type) => Logger.GetLoggerFactory().CreateLogger(type);

		/// <summary>
		/// Creates a logger
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static ILogger CreateLogger<T>() => Logger.CreateLogger(typeof(T));

		/// <summary>
		/// Writes a trace log message
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message">The log message</param>
		/// <param name="exception">The exception</param>
		public static void LogTrace(this ILogger logger, string message, Exception exception = null) => logger.LogTrace(exception, message);

		/// <summary>
		/// Writes a warning log message
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message">The log message</param>
		/// <param name="exception">The exception</param>
		public static void LogWarning(this ILogger logger, string message, Exception exception = null) => logger.LogWarning(exception, message);

		/// <summary>
		/// Writes a information log message
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message">The log message</param>
		/// <param name="exception">The exception</param>
		public static void LogInformation(this ILogger logger, string message, Exception exception = null) => logger.LogInformation(exception, message);

		/// <summary>
		/// Writes a debug log message
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message">The log message</param>
		/// <param name="exception">The exception</param>
		public static void LogDebug(this ILogger logger, string message, Exception exception = null) => logger.LogDebug(exception, message);

		/// <summary>
		/// Writes a error log message
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message">The log message</param>
		/// <param name="exception">The exception</param>
		public static void LogError(this ILogger logger, string message, Exception exception = null) => logger.LogError(exception, message);

		/// <summary>
		/// Writes a critical log message
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="message">The log message</param>
		/// <param name="exception">The exception</param>
		public static void LogCritical(this ILogger logger, string message, Exception exception = null) => logger.LogCritical(exception, message);

		/// <summary>
		/// Writes a log message
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="mode">Write mode</param>
		/// <param name="message">The log message</param>
		/// <param name="exception">The exception</param>
		public static void Log(this ILogger logger, LogLevel mode, string message, Exception exception = null)
		{
			switch (mode)
			{
				case LogLevel.Trace:
					logger.LogTrace(message, exception);
					break;

				case LogLevel.Information:
					logger.LogInformation(message, exception);
					break;

				case LogLevel.Warning:
					logger.LogWarning(message, exception);
					break;

				case LogLevel.Error:
					logger.LogError(message, exception);
					break;

				case LogLevel.Critical:
					logger.LogCritical(message, exception);
					break;

				default:
					logger.LogDebug(message, exception);
					break;
			}
		}

		/// <summary>
		/// Writes a log message
		/// </summary>
		/// <param name="logger"></param>
		/// <param name="minLevel">The minimum level (for checking when write)</param>
		/// <param name="mode">Write mode</param>
		/// <param name="message">The log message</param>
		/// <param name="exception">The exception</param>
		public static void Log(this ILogger logger, LogLevel minLevel, LogLevel mode, string message, Exception exception = null)
		{
			if (logger.IsEnabled(minLevel))
				logger.Log(mode, message, exception);
		}

		/// <summary>
		/// Writes a log message
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="minLevel">The minimum level (for checking when write)</param>
		/// <param name="mode">Write mode</param>
		/// <param name="message">The log message</param>
		/// <param name="exception">The exception</param>
		public static void Log<T>(LogLevel minLevel, LogLevel mode, string message, Exception exception = null) => Logger.CreateLogger<T>().Log(minLevel, mode, message, exception);
	}
}