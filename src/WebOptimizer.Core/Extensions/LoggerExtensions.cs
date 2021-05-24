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
        private static Action<ILogger, string, Exception> _logServedFromMemoryCache = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1001,
            formatString: "Responding from memory cache for '{Path}'");
        private static Action<ILogger, string, Exception> _logServedFromDiskCache = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1002,
            formatString: "Responding from disk cache for '{Path}'");
        private static Action<ILogger, string, Exception> _logGeneratedOutput = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1003,
            formatString: "Generated output and responded to request for '{Path}'");
        private static Action<ILogger, string, Exception> _logZeroByteResponse = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1004,
            formatString: "No response generated for '{Path}'. Passing on to next middleware.");
        private static Action<ILogger, string, Exception> _logFileNotFound = LoggerMessage.Define<string>(
            logLevel: LogLevel.Warning,
            eventId: 1005,
            formatString: "File '{Path}' not found. Passing on to next middleware.");
        
        public static void LogRequestForAssetStarted(this ILogger logger, string path) =>
            _logRequestForAssetStarted(logger, path, null);

        public static void LogServedFromMemoryCache(this ILogger logger, string path) =>
            _logServedFromMemoryCache(logger, path, null);

        public static void LogServedFromDiskCache(this ILogger logger, string path) =>
            _logServedFromDiskCache(logger, path, null);

        public static void LogGeneratedOutput(this ILogger logger, string path) =>
            _logGeneratedOutput(logger, path, null);

        public static void LogZeroByteResponse(this ILogger logger, string path) =>
            _logZeroByteResponse(logger, path, null);
        
        public static void LogFileNotFound(this ILogger logger, string path) =>
            _logFileNotFound(logger, path, null);
    }
}
