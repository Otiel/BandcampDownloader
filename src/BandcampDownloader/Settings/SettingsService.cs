using System;
using BandcampDownloader.Core;
using Config.Net;

namespace BandcampDownloader.Settings;

internal interface ISettingsService
{
    void InitializeSettings();
    IUserSettings GetUserSettings();
}

public sealed class SettingsService : ISettingsService
{
    private IUserSettings _userSettings;

    public void InitializeSettings()
    {
        _userSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(Constants.UserSettingsFilePath).Build();

        if (string.IsNullOrEmpty(_userSettings.DownloadsPath))
        {
            // Its default value cannot be set in settings as it isn't determined by a constant function
            _userSettings.DownloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\{artist}\\{album}";
        }
    }

    public IUserSettings GetUserSettings()
    {
        return _userSettings;
    }
}
