using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BandcampDownloader.Core.DependencyInjection;
using BandcampDownloader.Net;
using NLog;
using WpfMessageBoxLibrary;

namespace BandcampDownloader.UI.Dialogs.Update;

internal sealed partial class UserControlChangelog
{
    private readonly IHttpService _httpService;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private const string CHANGELOG_URL = "https://raw.githubusercontent.com/Otiel/BandcampDownloader/master/CHANGELOG.md";

    public UserControlChangelog()
    {
        _httpService = DependencyInjectionHelper.GetService<IHttpService>();

        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async Task<string> DownloadChangelogAsync()
    {
        var httpClient = _httpService.CreateHttpClient();
        var changelog = await httpClient.GetStringAsync(CHANGELOG_URL);
        return changelog;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        string changelog;
        try
        {
            changelog = await DownloadChangelogAsync();
        }
        catch
        {
            changelog = string.Format(Properties.Resources.changelogDownloadError, CHANGELOG_URL);
        }

        MarkdownViewer.Markdown = changelog;
    }

    private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
    {
        var url = e.Parameter.ToString();
        if (string.IsNullOrWhiteSpace(url))
        {
            _logger.Log(LogLevel.Error, $"url is invalid: {url}");
            return;
        }

        try
        {
            Process.Start(url);
        }
        catch (Win32Exception ex) when (ex.Message == "The system cannot find the file specified")
        {
            // Probably a relative link like "/docs/help-translate.md"
            var msgProperties = new WpfMessageBoxProperties
            {
                Button = MessageBoxButton.OK,
                ButtonOkText = Properties.Resources.messageBoxButtonOK,
                Image = MessageBoxImage.Error,
                Text = string.Format(Properties.Resources.messageBoxCouldNotOpenUrlError, url),
                Title = "Bandcamp Downloader",
            };
            WpfMessageBox.Show(Window.GetWindow(this), ref msgProperties);
        }
    }
}
