namespace BandcampDownloader;

internal interface IUserControlSettings
{
    void CancelChanges();

    void LoadSettings();

    void SaveSettings();
}
