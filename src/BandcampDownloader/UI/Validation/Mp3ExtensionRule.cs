using System.Globalization;
using System.Windows.Controls;

namespace BandcampDownloader {

    internal class Mp3ExtensionRule: ValidationRule {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            string valueString = value.ToString();

            if (valueString.Length < 4 || valueString.Substring(valueString.Length - 4, 4) != ".mp3") {
                return new ValidationResult(false, "Must end with '.mp3'");
            } else {
                return new ValidationResult(true, null);
            }
        }
    }
}