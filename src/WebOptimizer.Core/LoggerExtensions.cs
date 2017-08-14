using System;
using Microsoft.Extensions.Logging;

namespace WebOptimizer
{
    internal static class LoggerExtensions
    {
        private static Action<ILogger, string, Exception> _logRequestForAssetStarted = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1000,
            formatString: "Request started for '{Path}'");
        private static Action<ILogger, string, Exception> _logConditionalGet = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1001,
            formatString: "Responding with a conditional GET for '{Path}'");
        private static Action<ILogger, string, Exception> _logServedFromCache = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1002,
            formatString: "Responding from memory cache for '{Path}'");
        private static Action<ILogger, string, Exception> _logGeneratedOutput = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1003,
            formatString: "Generated output and responded to request for '{Path}'");

        public static void LogRequestForAssetStarted(this ILogger logger, string path)
        {
            _logRequestForAssetStarted(logger, path, null);
        }
        public static void LogConditionalGet(this ILogger logger, string path)
        {
            _logConditionalGet(logger, path, null);
        }
        public static void LogServedFromCache(this ILogger logger, string path)
        {
            _logServedFromCache(logger, path, null);
        }
        public static void LogGeneratedOutput(this ILogger logger, string path)
        {
            _logGeneratedOutput(logger, path, null);
        }
    }
}
