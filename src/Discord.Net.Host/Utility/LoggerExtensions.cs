using Microsoft.Extensions.Logging;
namespace Discord.Net.Host.Utility; 

public static class LoggerExtensions {
    /// <summary>
    /// Log a <see cref="LogMessage"/> to the <see cref="ILogger"/>
    /// </summary>
    /// <param name="logger">Logger to use</param>
    /// <param name="message">Discord log message to use</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static void LogMessage(this ILogger logger, LogMessage message) {
        var logLevel = message.Severity switch {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => throw new ArgumentOutOfRangeException()
        };

        logger.Log(logLevel, message.Exception, "[{Source}]: {Message}", message.Source, message.Message);
    }
}
