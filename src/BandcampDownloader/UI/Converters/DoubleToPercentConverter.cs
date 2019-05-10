using System;
using System.Globalization;
using System.Windows.Data;

namespace BandcampDownloader {

    /// /// <summary>
    /// Represents the converter that transforms Double values to and from their percent value. For instance: "0.05 → 5".
    /// </summary>
    internal class DoubleToPercentConverter: IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (double) value * 100; // 0.05 → 5%
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return double.Parse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture) / 100; // 5% → 0.05
        }
    }
}