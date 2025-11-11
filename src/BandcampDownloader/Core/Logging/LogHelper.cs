using System;
using System.Windows.Media;
using BandcampDownloader.Bandcamp.Download;
using BandcampDownloader.Core.DependencyInjection;
using BandcampDownloader.Settings;
using NLog;

namespace BandcampDownloader.Core.Logging;

internal static class LogHelper
{
    /// <summary>
    /// Returns the <see cref="SolidColorBrush"/> associated to the specified <see cref="DownloadProgressChangedLevel"/>.
    /// </summary>
    public static SolidColorBrush GetColor(DownloadProgressChangedLevel downloadProgressChangedLevel)
    {
        var userSettings = DependencyInjectionHelper.GetService<ISettingsService>().GetUserSettings();

        var color = downloadProgressChangedLevel switch
        {
            DownloadProgressChangedLevel.Info => userSettings.Theme == Skin.Dark ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f0f0")!) : Brushes.Black,
            DownloadProgressChangedLevel.VerboseInfo => userSettings.Theme == Skin.Dark ? Brushes.LightSkyBlue : Brushes.MediumBlue,
            DownloadProgressChangedLevel.IntermediateSuccess => userSettings.Theme == Skin.Dark ? Brushes.LightSkyBlue : Brushes.MediumBlue,
            DownloadProgressChangedLevel.Success => userSettings.Theme == Skin.Dark ? Brushes.Lime : Brushes.Green,
            DownloadProgressChangedLevel.Warning => userSettings.Theme == Skin.Dark ? Brushes.Orange : Brushes.OrangeRed,
            DownloadProgressChangedLevel.Error => Brushes.Red,
            _ => throw new ArgumentOutOfRangeException(nameof(downloadProgressChangedLevel), downloadProgressChangedLevel, null),
        };

        return color;
    }

    /// <summary>
    /// Returns the <see cref="LogLevel" /> associated to the specified <see cref="DownloadProgressChangedLevel" />.
    /// </summary>
    public static LogLevel ToNLogLevel(this DownloadProgressChangedLevel downloadProgressChangedLevel)
    {
        return downloadProgressChangedLevel switch
        {
            DownloadProgressChangedLevel.VerboseInfo => LogLevel.Debug,
            DownloadProgressChangedLevel.Info => LogLevel.Info,
            DownloadProgressChangedLevel.IntermediateSuccess => LogLevel.Info,
            DownloadProgressChangedLevel.Success => LogLevel.Info,
            DownloadProgressChangedLevel.Warning => LogLevel.Warn,
            DownloadProgressChangedLevel.Error => LogLevel.Error,
            _ => throw new ArgumentOutOfRangeException(nameof(downloadProgressChangedLevel), downloadProgressChangedLevel, null),
        };
    }
}
