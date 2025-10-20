﻿using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BandcampDownloader;

public partial class UserControlSettingsAbout : UserControl
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
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        e.Handled = true;
    }
}
