using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace BandcampDownloader {

    public class OnlyDigitsValidationRule: ValidationRule {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (Double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out Double doubleValue)) {
                return new ValidationResult(true, null);
            } else {
                return new ValidationResult(false, "Not a numeric value");
            }
        }
    }
}