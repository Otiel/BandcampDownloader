using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfMessageBoxLibrary;

namespace BandcampDownloader;

public partial class UserControlChangelog : UserControl
{
    public UserControlChangelog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Downloads the changelog file and returns its content.
    /// </summary>
    private async Task<string> DownloadChangelogAsync()
    {
        string changelog;
        using (var webClient = new WebClient { Encoding = Encoding.UTF8 })
        {
            ProxyHelper.SetProxy(webClient);
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

        markdownViewer.Markdown = changelog;
    }

    private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e)
    {
        var url = e.Parameter.ToString();

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
