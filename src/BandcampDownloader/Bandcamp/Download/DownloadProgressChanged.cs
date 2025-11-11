using System;

namespace BandcampDownloader.Bandcamp.Download;

internal delegate void DownloadProgressChangedEventHandler(object sender, DownloadProgressChangedArgs eventArgs);

internal sealed class DownloadProgressChangedArgs : EventArgs
{
    public DownloadProgressChangedLevel Level { get; private set; }
    public string Message { get; private set; }

    public DownloadProgressChangedArgs(string message, DownloadProgressChangedLevel level)
    {
        Message = message;
        Level = level;
    }
}

internal enum DownloadProgressChangedLevel
{
    VerboseInfo,
    Info,
    IntermediateSuccess,
    Success,
    Warning,
    Error,
}
