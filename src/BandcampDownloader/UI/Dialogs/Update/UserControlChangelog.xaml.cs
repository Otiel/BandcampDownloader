using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BandcampDownloader.Core;
using BandcampDownloader.DependencyInjection;
using BandcampDownloader.Net;
using NLog;
using WpfMessageBoxLibrary;

namespace BandcampDownloader.UI.Dialogs.Update;

internal sealed partial class UserControlChangelog
{
    private readonly IHttpService _httpService;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public UserControlChangelog()
    {
        _httpService = DependencyInjectionHelper.GetService<IHttpService>();

        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Downloads the changelog file and returns its content.
    /// </summary>
    private async Task<string> DownloadChangelogAsync()
    {
        string changelog;
#pragma warning disable SYSLIB0014
        using (var webClient = new WebClient())
#pragma warning restore SYSLIB0014
        {
            webClient.Encoding = Encoding.UTF8;
            _httpService.SetProxy(webClient);
            changelog = await webClient.DownloadStringTaskAsync(Constants.UrlChangelog);
        }

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
            changelog = string.Format(Properties.Resources.changelogDownloadError, Constants.UrlChangelog);
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
