using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BandcampDownloader.UI.Converters;

/// <summary>
/// Inverts Boolean values then converts them to and from Visibility enumeration values.
/// </summary>
internal class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is true ? Visibility.Hidden : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (Visibility?)value switch
        {
            Visibility.Collapsed => true,
            Visibility.Hidden => true,
            Visibility.Visible => false,
            null => false,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };
    }
}
