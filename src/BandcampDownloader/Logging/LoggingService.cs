using System.IO;
using System.Reflection;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace BandcampDownloader.Logging;

internal interface ILoggingService
{
    void InitializeLogger();
}

public sealed class LoggingService : ILoggingService
{
    /// <summary>
    /// The absolute path to the log file.
    /// </summary>
    private static readonly string LOG_FILE_PATH = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\BandcampDownloader.log";

    /// <summary>
    /// The log file maximum size in bytes.
    /// </summary>
    private const long MAX_LOG_SIZE = 1024 * 1024;

    public void InitializeLogger()
    {
        var fileTarget = new FileTarget
        {
            FileName = LOG_FILE_PATH,
            Layout = "${longdate}  ${level:uppercase=true:padding=-5:padCharacter= }  ${logger}  ${message}  ${exception:format=tostring}",
            ArchiveAboveSize = MAX_LOG_SIZE,
            MaxArchiveFiles = 1,
        };

        var config = new LoggingConfiguration();
        config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);

        LogManager.Configuration = config;
    }
}
