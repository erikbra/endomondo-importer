using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace endoimport.Logging
{
    [ProviderAlias("FileLogger")]
    public class FileLoggerProvider : ILoggerProvider
    {
        public readonly FileLoggerOptions Options;
 
        public FileLoggerProvider(IOptions<FileLoggerOptions> options)
        {
            Options = options.Value;
        }
 
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(this);
        }
 
        public void Dispose()
        {
        }
    }
    
}