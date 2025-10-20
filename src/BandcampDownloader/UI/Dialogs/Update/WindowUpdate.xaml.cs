using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using BandcampDownloader.Core;
using BandcampDownloader.Helpers;
using Microsoft.Win32;

namespace BandcampDownloader.UI.Dialogs.Update;

public sealed partial class WindowUpdate : Window
{
    private Version _latestVersion;

    public WindowUpdate()
    {
        InitializeComponent();
    }

    private async void ButtonDownloadUpdate_Click(object sender, RoutedEventArgs e)
    {
        var parts = Constants.UrlReleaseZip.Split(new[] { '/' });
        var defaultFileName = parts[parts.Length - 1];

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

        var path = dialog.FileName;
        var zipUrl = string.Format(Constants.UrlReleaseZip, _latestVersion.ToString());

        using (var webClient = new WebClient())
        {
            ProxyHelper.SetProxy(webClient);
            await webClient.DownloadFileTaskAsync(zipUrl, path);
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        e.Handled = true;
    }

    private async void WindowUpdate_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _latestVersion = await UpdatesHelper.GetLatestVersionAsync();
        }
        catch (CouldNotCheckForUpdatesException)
        {
            // Do nothing, the button will stayed disabled
            return;
        }

        var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
        if (currentVersion.CompareTo(_latestVersion) < 0)
        {
            // The latest version is newer than the current one
            buttonDownloadUpdate.IsEnabled = true;
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
