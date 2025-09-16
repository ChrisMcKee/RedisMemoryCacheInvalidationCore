using System;
using Microsoft.Extensions.Logging;

namespace RedisMemoryCacheInvalidation.Utils
{
    /// <summary>
    /// Provides safe logging functionality that handles null loggers gracefully.
    /// </summary>
    internal static class SafeLogger
    {
        /// <summary>
        /// Logs a trace message safely.
        /// </summary>
        /// <param name="logger">The logger instance (can be null).</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void LogTrace(ILogger logger, string message, params object[] args)
        {
            if(logger == null || !logger.IsEnabled(LogLevel.Trace)) return;
            logger?.LogTrace(message, args);
        }

        /// <summary>
        /// Logs a trace message safely with an exception.
        /// </summary>
        /// <param name="logger">The logger instance (can be null).</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void LogTrace(ILogger logger, Exception exception, string message, params object[] args)
        {
            if(logger == null || !logger.IsEnabled(LogLevel.Trace)) return;
            logger?.LogTrace(exception, message, args);
        }

        /// <summary>
        /// Logs a debug message safely.
        /// </summary>
        /// <param name="logger">The logger instance (can be null).</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void LogDebug(ILogger logger, string message, params object[] args)
        {
            if(logger == null || !logger.IsEnabled(LogLevel.Debug)) return;
            logger?.LogDebug(message, args);
        }

        /// <summary>
        /// Logs an information message safely.
        /// </summary>
        /// <param name="logger">The logger instance (can be null).</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void LogInformation(ILogger logger, string message, params object[] args)
        {
            if(logger == null || !logger.IsEnabled(LogLevel.Information)) return;
            logger?.LogInformation(message, args);
        }

        /// <summary>
        /// Logs a warning message safely.
        /// </summary>
        /// <param name="logger">The logger instance (can be null).</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void LogWarning(ILogger logger, string message, params object[] args)
        {
            if(logger == null || !logger.IsEnabled(LogLevel.Warning)) return;
            logger?.LogWarning(message, args);
        }

        /// <summary>
        /// Logs a warning message safely with an exception.
        /// </summary>
        /// <param name="logger">The logger instance (can be null).</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void LogWarning(ILogger logger, Exception exception, string message, params object[] args)
        {
            if(logger == null || !logger.IsEnabled(LogLevel.Warning)) return;
            logger?.LogWarning(exception, message, args);
        }

        /// <summary>
        /// Logs an error message safely.
        /// </summary>
        /// <param name="logger">The logger instance (can be null).</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void LogError(ILogger logger, Exception exception, string message, params object[] args)
        {
            if(logger == null || !logger.IsEnabled(LogLevel.Error)) return;
            logger?.LogError(exception, message, args);
        }

        /// <summary>
        /// Logs an error message safely without an exception.
        /// </summary>
        /// <param name="logger">The logger instance (can be null).</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void LogError(ILogger logger, string message, params object[] args)
        {
            if(logger == null || !logger.IsEnabled(LogLevel.Error)) return;
            logger?.LogError(message, args);
        }

        /// <summary>
        /// Logs a critical error message safely.
        /// </summary>
        /// <param name="logger">The logger instance (can be null).</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void LogCritical(ILogger logger, Exception exception, string message, params object[] args)
        {
            if(logger == null || !logger.IsEnabled(LogLevel.Critical)) return;
            logger?.LogCritical(exception, message, args);
        }

        /// <summary>
        /// Logs a critical error message safely without an exception.
        /// </summary>
        /// <param name="logger">The logger instance (can be null).</param>
        /// <param name="message">The message to log.</param>
        /// <param name="args">Optional format arguments.</param>
        public static void LogCritical(ILogger logger, string message, params object[] args)
        {
            if(logger == null || !logger.IsEnabled(LogLevel.Critical)) return;
            logger?.LogCritical(message, args);
        }
    }
}
