﻿using System.Globalization;
using System.Windows.Controls;

namespace BandcampDownloader.UI.Validation;

internal sealed class DoubleRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (double.TryParse(value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            return new ValidationResult(true, null);
        }

        return new ValidationResult(false, "Not a numeric value");
    }
}
