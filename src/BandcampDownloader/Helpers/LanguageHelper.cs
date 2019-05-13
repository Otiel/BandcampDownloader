using System.Globalization;
using WPFLocalizeExtension.Engine;

namespace BandcampDownloader {

    internal static class LanguageHelper {

        public static void ApplyLanguage(string cultureName) {
            // Apply language
            LocalizeDictionary.Instance.Culture = new CultureInfo(cultureName);

            // Set system MessageBox buttons
            MessageBoxManager.Unregister();
            MessageBoxManager.OK = Properties.Resources.messageBoxButtonOK;
            MessageBoxManager.Cancel = Properties.Resources.messageBoxButtonCancel;
            MessageBoxManager.Yes = Properties.Resources.messageBoxButtonYes;
            MessageBoxManager.No = Properties.Resources.messageBoxButtonNo;
            MessageBoxManager.Register();
        }
    }
}