using System;
using System.Globalization;
using System.Windows.Data;

namespace BandcampDownloader {

    /// <summary>
    /// Converts Boolean values to their logical AND operation result.
    /// </summary>
    internal class BooleanAndConverter: IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            foreach (object value in values) {
                if ((value is bool) && (bool) value == false) {
                    return false;
                }
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            return null;
        }
    }
}