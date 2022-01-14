using Serilog;
using Serilog.Events;

namespace Bcdl.Console;

public sealed class Logger : ILogger
{
    private readonly Serilog.Core.Logger _logger;

    public Logger()
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .WriteTo.File(@"C:\Users\Leito\Desktop\bcdl.console.log", fileSizeLimitBytes: 10_000, rollOnFileSizeLimit: true)
            .CreateLogger();
    }

    public void Write(LogEvent logEvent)
    {
        _logger.Write(logEvent);
    }
}
