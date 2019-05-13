using System.Globalization;
using WPFLocalizeExtension.Engine;

namespace BandcampDownloader {

    internal static class LanguageHelper {

        /// <summary>
        /// Applies the specified language.
        /// </summary>
        /// <param name="language">The language to apply.</param>
        public static void ApplyLanguage(Language language) {
            // Apply language
            LocalizeDictionary.Instance.Culture = new CultureInfo(language.ToString());

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