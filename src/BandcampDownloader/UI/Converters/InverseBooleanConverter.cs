using System;
using System.Globalization;
using System.Windows.Data;

namespace BandcampDownloader {

    /// <summary>
    /// Converts Boolean values to and from their inverse Boolean values.
    /// </summary>
    internal class InverseBooleanConverter: IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return !(bool) value;
        }
    }
}