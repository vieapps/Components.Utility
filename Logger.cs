using System;
using Microsoft.Extensions.Logging;

namespace net.vieapps.Components.Utility
{
	public static class Logger
	{
		static ILoggerFactory LoggerFactory;

		/// <summary>
		/// Assigns a logger factory
		/// </summary>
		/// <param name="loggerFactory"></param>
		public static void AssignLoggerFactory(ILoggerFactory loggerFactory)
		{
			if (Logger.LoggerFactory == null && loggerFactory != null)
				Logger.LoggerFactory = loggerFactory;
		}

		/// <summary>
		/// Gets a logger factory
		/// </summary>
		/// <returns></returns>
		public static ILoggerFactory GetLoggerFactory()
		{
			return Logger.LoggerFactory ?? new NullLoggerFactory();
		}

		/// <summary>
		/// Creates a logger
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static ILogger CreateLogger(Type type)
		{
			return Logger.GetLoggerFactory().CreateLogger(type);
		}

		/// <summary>
		/// Creates a logger
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static ILogger CreateLogger<T>()
		{
			return Logger.CreateLogger(typeof(T));
		}

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
					if (exception != null)
						logger.LogTrace(exception, message);
					else
						logger.LogTrace(message);
					break;

				case LogLevel.Information:
					if (exception != null)
						logger.LogInformation(exception, message);
					else
						logger.LogInformation(message);
					break;

				case LogLevel.Warning:
					if (exception != null)
						logger.LogError(exception, message);
					else
						logger.LogError(message);
					break;

				case LogLevel.Error:
					if (exception != null)
						logger.LogError(exception, message);
					else
						logger.LogError(message);
					break;

				case LogLevel.Critical:
					if (exception != null)
						logger.LogCritical(exception, message);
					else
						logger.LogCritical(message);
					break;

				default:
					if (exception != null)
						logger.LogDebug(exception, message);
					else
						logger.LogDebug(message);
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
		public static void Log<T>(LogLevel minLevel, LogLevel mode, string message, Exception exception = null)
		{
			Logger.CreateLogger<T>().Log(minLevel, mode, message, exception);
		}
	}

	#region NullLogger
	internal class NullLoggerFactory : ILoggerFactory
	{
		public void AddProvider(ILoggerProvider provider) { }

		public ILogger CreateLogger(string categoryName)
		{
			return NullLogger.Instance;
		}

		public void Dispose() { }
	}

	public class NullLogger : ILogger
	{
		internal static NullLogger Instance = new NullLogger();

		private NullLogger() { }

		public IDisposable BeginScope<TState>(TState state) { return null; }

		public bool IsEnabled(LogLevel logLevel)
		{
			return false;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }
	}
	#endregion

}