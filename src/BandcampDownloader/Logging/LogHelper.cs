using System;
using System.Windows.Media;
using BandcampDownloader.Core;
using NLog;

namespace BandcampDownloader.Logging;

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
            LogType.Info => App.UserSettings.Theme == Skin.Dark ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0")!) : Brushes.Black,
            LogType.VerboseInfo => App.UserSettings.Theme == Skin.Dark ? Brushes.LightSkyBlue : Brushes.MediumBlue,
            LogType.IntermediateSuccess => App.UserSettings.Theme == Skin.Dark ? Brushes.LightSkyBlue : Brushes.MediumBlue,
            LogType.Success => App.UserSettings.Theme == Skin.Dark ? Brushes.Lime : Brushes.Green,
            LogType.Warning => App.UserSettings.Theme == Skin.Dark ? Brushes.Orange : Brushes.OrangeRed,
            LogType.Error => Brushes.Red,
            _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
        };

        return color;
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
            _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
        };
    }
}
