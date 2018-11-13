using System;
using System.Globalization;
using System.Windows.Data;

namespace BandcampDownloader {

    /// <summary>
    /// Transforms a double to its percent value. For instance: "0.05 → 5".
    /// </summary>
    public class DoubleToPercentConverter: IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!( value is Double percentValue )) {
                // Shouldn't happen
                throw new ArgumentException("Value should be a Double", nameof(value));
            }
            return percentValue * 100; // 0.05 → 5%
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!Double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out Double doubleValue)) {
                // Shouldn't happen
                throw new ArgumentException("Value should be a Double", nameof(value));
            }

            return doubleValue / 100; // 5% → 0.05
        }
    }
}