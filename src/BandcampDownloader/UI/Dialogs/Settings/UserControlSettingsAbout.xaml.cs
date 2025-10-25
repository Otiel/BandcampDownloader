using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using BandcampDownloader.Helpers;
using BandcampDownloader.UI.Dialogs.Update;

namespace BandcampDownloader.UI.Dialogs.Settings;

internal sealed partial class UserControlSettingsAbout
{
    public UserControlSettingsAbout()
    {
        InitializeComponent();
    }

    private void ButtonViewChangelog_Click(object sender, RoutedEventArgs e)
    {
        var windowUpdate = new WindowUpdate
        {
            Owner = (Window)((Grid)Parent).Parent,
            ShowInTaskbar = true,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };
        windowUpdate.Show();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        UrlHelper.OpenUrlInBrowser(e.Uri.AbsoluteUri);
        e.Handled = true;
    }
}
