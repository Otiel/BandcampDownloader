﻿using System.Windows.Controls;

namespace BandcampDownloader
{
    public partial class UserControlSettingsDownloads : UserControl, IUserControlSettings
    {
        public UserControlSettingsDownloads()
        {
            InitializeComponent();
            // Save data context for bindings
            DataContext = App.UserSettings;
        }

        /// <summary>
        /// Cancels the changes already applied.
        /// </summary>
        public void CancelChanges()
        {
            // Nothing to "unapply"
        }

        /// <summary>
        /// Loads settings from App.UserSettings.
        /// </summary>
        public void LoadSettings()
        {
            // Reload DataContext in case settings have changed
            DataContext = App.UserSettings;
            // No need to call UpdateTarget, it is done automatically
        }

        /// <summary>
        /// Saves settings to App.UserSettings.
        /// </summary>
        public void SaveSettings()
        {
            checkBoxOneAlbumAtATime.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            checkBoxRetrieveFilesSize.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
        }
    }
}