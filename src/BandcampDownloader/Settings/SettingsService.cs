using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using Config.Net;

namespace BandcampDownloader.Settings;

internal interface ISettingsService
{
    IUserSettings InitializeSettings();
    IUserSettings ResetSettings();
    IUserSettings GetUserSettings();
    string GetUserSettingsInJson();
}

public sealed class SettingsService : ISettingsService
{
    /// <summary>
    /// The absolute path to the settings file.
    /// </summary>
    private static readonly string USER_SETTINGS_FILE_PATH = Directory.GetParent(Assembly.GetExecutingAssembly().Location) + @"\BandcampDownloader.ini";

    private IUserSettings _userSettings;

    public IUserSettings InitializeSettings()
    {
        _userSettings = new ConfigurationBuilder<IUserSettings>().UseIniFile(USER_SETTINGS_FILE_PATH).Build();

        if (string.IsNullOrEmpty(_userSettings.DownloadsPath))
        {
            // Its default value cannot be set in settings as it isn't determined by a constant function
            _userSettings.DownloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\{artist}\\{album}";
        }

        return _userSettings;
    }

    public IUserSettings ResetSettings()
    {
        File.Delete(USER_SETTINGS_FILE_PATH);
        _userSettings = InitializeSettings();

        return _userSettings;
    }

    public IUserSettings GetUserSettings()
    {
        return _userSettings;
    }

    public string GetUserSettingsInJson()
    {
        var serializedSettings = JsonSerializer.Serialize(_userSettings);
        return serializedSettings;
    }
}
