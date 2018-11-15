using System;
using System.Globalization;
using System.Windows.Data;

namespace BandcampDownloader {

    /// <summary>
    /// Converts Boolean values to and from their inverse Boolean values.
    /// </summary>
    public class InverseBooleanConverter: IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return !((Boolean) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return !((Boolean) value);
        }
    }
}