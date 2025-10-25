using System;
using System.Windows;
using BandcampDownloader.Core;
using BandcampDownloader.Properties;
using NLog;

namespace BandcampDownloader.Helpers;

internal interface IExceptionHandler
{
    void RegisterUnhandledExceptionHandler();
}

public sealed class ExceptionHandler : IExceptionHandler
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public void RegisterUnhandledExceptionHandler()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogExceptionAndInnerExceptionsRecursively((Exception)e.ExceptionObject);

        MessageBox.Show(string.Format(Resources.messageBoxUnhandledException, Constants.UrlIssues), "Bandcamp Downloader", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Writes the specified <see cref="Exception" /> and all its InnerExceptions to the log.
    /// </summary>
    private void LogExceptionAndInnerExceptionsRecursively(Exception exception)
    {
        _logger.Log(LogLevel.Fatal, $"{exception.GetType()} {exception.Message}");
        _logger.Log(LogLevel.Fatal, exception.StackTrace ?? "No stack trace available");

        if (exception.InnerException != null)
        {
            LogExceptionAndInnerExceptionsRecursively(exception.InnerException);
        }
    }
}
