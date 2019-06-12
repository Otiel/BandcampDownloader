using System;
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
            LocalizeDictionary.Instance.Culture = GetCultureInfo(language);
        }

        /// <summary>
        /// Returns the CultureInfo corresponding to the specified Language.
        /// </summary>
        /// <param name="language">The Language.</param>
        private static CultureInfo GetCultureInfo(Language language) {
            switch (language) {
                case Language.en:
                    return new CultureInfo("en");
                case Language.de:
                    return new CultureInfo("de");
                case Language.es:
                    return new CultureInfo("es");
                case Language.fr:
                    return new CultureInfo("fr");
                case Language.it:
                    return new CultureInfo("it");
                case Language.nb_NO:
                    return new CultureInfo("nb-NO");
                case Language.pl:
                    return new CultureInfo("pl");
                case Language.tr:
                    return new CultureInfo("tr");
                case Language.uk:
                    return new CultureInfo("uk");
                default:
                    throw new NotImplementedException();
            }
        }
    }
}