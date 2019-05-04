using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BandcampDownloader {

    public partial class UserControlChangelog: UserControl {

        public UserControlChangelog() {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Downloads the changelog file and returns its content.
        /// </summary>
        private async Task<String> DownloadChangelogAsync() {
            String changelog;
            using (var webClient = new WebClient() { Encoding = Encoding.UTF8 }) {
                ProxyHelper.SetProxy(webClient);
                changelog = await webClient.DownloadStringTaskAsync(Constants.ChangelogUrl);
            }

            return changelog;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e) {
            String changelog;
            try {
                changelog = await DownloadChangelogAsync();
            } catch {
                changelog = String.Format(Properties.Resources.changelogDownloadError, Constants.ChangelogUrl);
            }

            markdownViewer.Markdown = changelog;
        }

        private void OpenHyperlink(object sender, ExecutedRoutedEventArgs e) {
            Process.Start(e.Parameter.ToString());
        }
    }
}