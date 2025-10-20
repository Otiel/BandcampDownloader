using System;
using System.Globalization;
using BandcampDownloader.Core;
using WPFLocalizeExtension.Engine;

namespace BandcampDownloader.Helpers;

internal static class LanguageHelper
{
    /// <summary>
    /// Applies the specified language.
    /// </summary>
    /// <param name="language">The language to apply.</param>
    public static void ApplyLanguage(Language language)
    {
        // Apply language
        LocalizeDictionary.Instance.Culture = GetCultureInfo(language);
    }

    /// <summary>
    /// Returns the CultureInfo corresponding to the specified Language.
    /// </summary>
    /// <param name="language">The Language.</param>
    private static CultureInfo GetCultureInfo(Language language)
    {
        // Existing cultures: https://dotnetfiddle.net/e1BX7M
        switch (language)
        {
            case Language.ar:
                return new CultureInfo("ar");
            case Language.ca:
                return new CultureInfo("ca");
            case Language.de:
                return new CultureInfo("de");
            case Language.en:
                return new CultureInfo("en");
            //case Language.eo:
            //    return new CultureInfo("eo");
            case Language.es:
                return new CultureInfo("es");
            case Language.fi:
                return new CultureInfo("fi");
            case Language.fr:
                return new CultureInfo("fr");
            case Language.hr:
                return new CultureInfo("hr");
            case Language.hu:
                return new CultureInfo("hu");
            case Language.id:
                return new CultureInfo("id");
            case Language.it:
                return new CultureInfo("it");
            // case Language.ja:
            //     return new CultureInfo("ja");
            //case Language.ko:
            //    return new CultureInfo("ko");
            case Language.nb_NO:
                return new CultureInfo("nb-NO");
            case Language.pl:
                return new CultureInfo("pl");
            case Language.nl:
                return new CultureInfo("nl");
            case Language.pt:
                return new CultureInfo("pt");
            case Language.pt_BR:
                return new CultureInfo("pt-BR");
            case Language.ru:
                return new CultureInfo("ru");
            case Language.sv:
                return new CultureInfo("sv");
            case Language.tr:
                return new CultureInfo("tr");
            case Language.uk:
                return new CultureInfo("uk");
            case Language.vi:
                return new CultureInfo("vi");
            case Language.zh:
                return new CultureInfo("zh");
            default:
                throw new NotImplementedException();
        }
    }
}
