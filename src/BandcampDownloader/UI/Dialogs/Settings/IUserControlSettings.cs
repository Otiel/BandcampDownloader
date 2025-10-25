using BandcampDownloader.Settings;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal interface IUserControlSettings
{
    void CancelChanges();

    void LoadSettings(IUserSettings userSettings);

    void SaveSettings();
}
