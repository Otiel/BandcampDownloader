namespace BandcampDownloader.UI.Dialogs.Settings;

internal interface IUserControlSettings
{
    void CancelChanges();

    void LoadSettings();

    void SaveSettings();
}
