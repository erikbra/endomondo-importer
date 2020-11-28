using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Logging;

namespace endoimport.Logging
{
    public class FileLogger : ILogger
    {
        private readonly FileLoggerProvider _loggerProvider;

        public FileLogger([NotNull] FileLoggerProvider loggerProvider)
        {
            _loggerProvider = loggerProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var file = _loggerProvider.Options.File;
            var timeStamp = DateTimeOffset.UtcNow.ToString("O");
            var level = logLevel.ToString();
            var message = formatter(state, exception);
            var stackTrace = exception?.StackTrace ?? string.Empty;
            
            var logRecord = $"[{timeStamp}] [{level}] {message} {stackTrace}";

            using var streamWriter = new StreamWriter(file, true);
            streamWriter.WriteLine(logRecord);
            streamWriter.Flush();
        }
    }
}