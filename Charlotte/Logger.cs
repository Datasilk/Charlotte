using System;
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Crayon;
using Microsoft.Extensions.Options;

namespace Charlotte
{
    public sealed class LoggerConfiguration
    {
        public int EventId { get; set; }

        public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
        {
            [LogLevel.Information] = ConsoleColor.Green
        };
    }

    [UnsupportedOSPlatform("browser")]
    [ProviderAlias("Charlotte")]
    public sealed class CharlotteLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable? _onChangeToken;
        private LoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, UpdaterLogger> _loggers =
            new(StringComparer.OrdinalIgnoreCase);

        public CharlotteLoggerProvider(IOptionsMonitor<LoggerConfiguration> config)
        {
            //_currentConfig = config.CurrentValue;
            //_onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new UpdaterLogger());

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }
    }

    public class UpdaterLogger : ILogger
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= this.LogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            string message = formatter(state, exception);
            if (exception != null)
                message += $"\n{exception}";

            switch (logLevel)
            {
                case LogLevel.Trace:
                    Console.Out.WriteLineAsync(message);
                    break;
                case LogLevel.Debug:
                    Console.Out.WriteLineAsync(message);
                    break;
                case LogLevel.Information:
                    Console.Out.WriteLineAsync(message);
                    break;
                case LogLevel.Warning:
                    Console.Out.WriteLineAsync(Output.Dim().Yellow().Text(message));
                    break;
                case LogLevel.Error:
                    Console.Error.WriteLineAsync(Output.Bright.Red(message));
                    break;
                case LogLevel.Critical:
                    Console.Error.WriteLineAsync(Output.Bright.Red(message));
                    break;
            }
        }
    }
}
