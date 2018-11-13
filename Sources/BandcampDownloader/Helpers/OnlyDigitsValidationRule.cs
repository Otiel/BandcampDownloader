using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace BandcampDownloader {

    public class OnlyDigitsValidationRule: ValidationRule {

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            var validationResult = new ValidationResult(true, null);

            if (value != null) {
                if (!String.IsNullOrEmpty(value.ToString())) {
                    var regex = new Regex("[^0-9.-]+"); // Regex that matches disallowed text
                    var parsingOk = !regex.IsMatch(value.ToString());
                    if (!parsingOk) {
                        validationResult = new ValidationResult(false, "Enter a numeric value");
                    }
                }
            }

            return validationResult;
        }
    }
}