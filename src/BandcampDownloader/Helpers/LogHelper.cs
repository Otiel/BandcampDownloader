using System;
using System.Windows.Media;
using BandcampDownloader.Core;
using NLog;

namespace BandcampDownloader.Helpers;

internal static class LogHelper
{
    /// <summary>
    /// Returns the color associated to the specified log type.
    /// </summary>
    /// <param name="logType">The type of the log.</param>
    public static SolidColorBrush GetColor(LogType logType)
    {
        var color = logType switch
        {
            LogType.Info => App.UserSettings.Theme == Skin.Dark ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0")) : Brushes.Black,
            LogType.VerboseInfo => App.UserSettings.Theme == Skin.Dark ? Brushes.LightSkyBlue : Brushes.MediumBlue,
            LogType.IntermediateSuccess => App.UserSettings.Theme == Skin.Dark ? Brushes.LightSkyBlue : Brushes.MediumBlue,
            LogType.Success => App.UserSettings.Theme == Skin.Dark ? Brushes.Lime : Brushes.Green,
            LogType.Warning => App.UserSettings.Theme == Skin.Dark ? Brushes.Orange : Brushes.OrangeRed,
            LogType.Error => App.UserSettings.Theme == Skin.Dark ? Brushes.Red : Brushes.Red,
            _ => throw new NotImplementedException(),
        };

        return color;
    }

    /// <summary>
    /// Writes the specified Exception and all its InnerException to the application log file.
    /// </summary>
    /// <param name="exception">The Exception to log.</param>
    public static void LogExceptionAndInnerExceptionsToFile(Exception exception)
    {
        LogExceptionToFile(exception);

        if (exception.InnerException != null)
        {
            LogExceptionAndInnerExceptionsToFile(exception.InnerException);
        }
    }

    /// <summary>
    /// Writes the specified Exception to the application log file.
    /// </summary>
    /// <param name="exception">The Exception to log.</param>
    public static void LogExceptionToFile(Exception exception)
    {
        var logger = LogManager.GetCurrentClassLogger();
        logger.Log(LogLevel.Fatal, string.Format("{0} {1}", exception.GetType(), exception.Message));
        logger.Log(LogLevel.Fatal, exception.StackTrace);
    }

    /// <summary>
    /// Returns the NLog.LogLevel associated to the specified LogType.
    /// </summary>
    public static LogLevel ToNLogLevel(this LogType logType)
    {
        return logType switch
        {
            LogType.VerboseInfo => LogLevel.Debug,
            LogType.Info => LogLevel.Info,
            LogType.IntermediateSuccess => LogLevel.Info,
            LogType.Success => LogLevel.Info,
            LogType.Warning => LogLevel.Warn,
            LogType.Error => LogLevel.Error,
            _ => throw new NotImplementedException(),
        };
    }
}
