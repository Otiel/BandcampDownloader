using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Helpers;
using BandcampDownloader.Net;
using BandcampDownloader.Updates;
using Microsoft.Win32;
using NLog;

namespace BandcampDownloader.UI.Dialogs.Update;

internal sealed partial class WindowUpdate
{
    private readonly IHttpService _httpService;
    private readonly IUpdatesService _updatesService;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private Version _latestVersion;

    public WindowUpdate()
    {
        _httpService = DependencyInjectionHelper.GetService<IHttpService>();
        _updatesService = DependencyInjectionHelper.GetService<IUpdatesService>();
        InitializeComponent();
    }

    private async void ButtonDownloadUpdate_Click(object sender, RoutedEventArgs e)
    {
        var latestReleaseAssetUrl = await _updatesService.GetLatestReleaseAssetUrlAsync();

        var parts = latestReleaseAssetUrl.Split(new[] { '/' });
        var defaultFileName = parts[^1];

        var dialog = new SaveFileDialog
        {
            FileName = defaultFileName,
            Filter = "Archive|*.zip",
            Title = Properties.Resources.saveFileDialogTitle,
        };
        var dialogResult = dialog.ShowDialog();
        if (dialogResult is false)
        {
            return;
        }

        var fileInfo = new FileInfo(dialog.FileName);

        await _httpService.DownloadFileAsync(latestReleaseAssetUrl, fileInfo, CancellationToken.None);
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        UrlHelper.OpenUrlInBrowser(e.Uri.AbsoluteUri);
        e.Handled = true;
    }

    private async void WindowUpdate_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _latestVersion = await _updatesService.GetLatestVersionAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get latest version");

            // Do nothing, the button will stay disabled
            return;
        }

        if (_latestVersion.IsNewerVersion())
        {
            ButtonDownloadUpdate.IsEnabled = true;
        }
    }

    private void WindowUpdate_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }
}
