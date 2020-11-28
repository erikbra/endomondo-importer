using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace endoimport.Logging
{
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFileLogger(
            this ILoggingBuilder builder, 
            Action<FileLoggerOptions> configure)
        {
            builder.Services
                .AddSingleton<ILoggerProvider, FileLoggerProvider>()
                .Configure(configure);
            return builder;
        }
    }
}