using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Navigation;

namespace BandcampDownloader {

    public partial class WindowUpdate: Window {
        private Version _latestVersion;

        public WindowUpdate() {
            InitializeComponent();
        }

        private async void ButtonDownloadUpdate_Click(object sender, RoutedEventArgs e) {
            String[] parts =  Constants.ZipUrl.Split(new char[] { '/' });
            String defaultFileName = parts[parts.Length - 1];

            var dialog = new SaveFileDialog {
                FileName = defaultFileName,
                Filter = "Archive|*.zip",
                Title = Properties.Resources.saveFileDialogTitle,
            };
            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) {
                return;
            }

            String path = dialog.FileName;
            String zipUrl = String.Format(Constants.ZipUrl, _latestVersion.ToString());

            using (var webClient = new WebClient()) {
                ProxyHelper.SetProxy(webClient);
                await webClient.DownloadFileTaskAsync(zipUrl, path);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void WindowUpdate_Loaded(object sender, RoutedEventArgs e) {
            try {
                _latestVersion = UpdatesHelper.GetLatestVersion();
            } catch (CouldNotCheckForUpdatesException) {
                // Do nothing
                return;
            }

            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (currentVersion.CompareTo(_latestVersion) < 0) {
                // The latest version is newer than the current one
                buttonDownloadUpdate.IsEnabled = true;
            }
        }

        private void WindowUpdate_Loaded_1(object sender, RoutedEventArgs e) {
        }

        private void WindowUpdate_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                Close();
            }
        }
    }
}