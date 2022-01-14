using System.ComponentModel;
using System.Diagnostics;
using Bcdl.Downloader.Helpers;
using Downloader;
using Serilog;

namespace Bcdl.Downloader.Services;

public interface IDownloadFileService
{
    Task DownloadFileAsync(string url, string filePath, CancellationToken ct);
    void PauseDownload();
    Task ResumeDownloadAsync();
}

public sealed class DownloadFileService : IDownloadFileService
{
    private readonly ILogger _logger;
    private readonly DownloadService _downloadService;
    private DownloadPackage? _downloadPackage;

    public DownloadFileService(ILogger logger)
    {
        _logger = logger;
        _downloadService = new DownloadService();
        _downloadService.DownloadStarted += OnDownloadStarted;
        _downloadService.DownloadProgressChanged += OnDownloadProgressChanged;
        _downloadService.DownloadFileCompleted += OnDownloadFileCompleted;
    }

    public async Task DownloadFileAsync(string url, string filePath, CancellationToken ct)
    {
        ct.Register(CancelDownload);
        await _downloadService.DownloadFileTaskAsync(url, filePath).ConfigureAwait(false);
    }

    public void PauseDownload()
    {
        _downloadPackage = _downloadService.Package;
        _downloadService.CancelAsync();
    }

    public async Task ResumeDownloadAsync()
    {
        if (_downloadPackage is null)
        {
            _logger.Error("No download package, have you paused the download before trying to resume it?");
            return;
        }

        await _downloadService.DownloadFileTaskAsync(_downloadPackage).ConfigureAwait(false);
    }

    private void CancelDownload()
    {
        _downloadService.CancelAsync();
    }

    private static void OnDownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
    {
        if (e.UserState is not DownloadPackage downloadPackage)
        {
            Debug.Fail($"{nameof(e.UserState)} is not a {typeof(DownloadPackage)}");
            return;
        }

        if (e.Cancelled)
        {
            Console.WriteLine($"Download cancelled for {downloadPackage.Address} - File: {downloadPackage.FileName} - Error: {e.Error}");
        }

        Console.WriteLine($"Download finished for for {downloadPackage.Address} - File: {downloadPackage.FileName}");
    }

    private static void OnDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        Console.WriteLine($"Downloading: {e.ReceivedBytesSize} / {e.TotalBytesToReceive} - Average speed: {UnitsHelper.BytesToKiloBytes(e.AverageBytesPerSecondSpeed)} kB/s");
    }

    private static void OnDownloadStarted(object? sender, DownloadStartedEventArgs e)
    {
        Console.WriteLine($"Starting download: expecting to receive {UnitsHelper.BytesToKiloBytes(e.TotalBytesToReceive)} kB to file: {e.FileName}");
    }
}
