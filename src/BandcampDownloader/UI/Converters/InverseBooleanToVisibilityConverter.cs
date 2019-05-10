using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BandcampDownloader {

    /// <summary>
    /// Inverts Boolean values then converts them to and from Visibility enumeration values.
    /// </summary>
    internal class InverseBooleanToVisibilityConverter: IValueConverter {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (bool) value ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            switch ((Visibility) value) {
                case Visibility.Collapsed:
                    return true;
                case Visibility.Hidden:
                    return true;
                case Visibility.Visible:
                    return false;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}