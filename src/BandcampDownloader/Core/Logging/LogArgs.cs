using System;

namespace BandcampDownloader.Core.Logging;

internal sealed class LogArgs : EventArgs
{
    public LogType LogType { get; private set; }
    public string Message { get; private set; }

    public LogArgs(string message, LogType logType)
    {
        Message = message;
        LogType = logType;
    }
}
