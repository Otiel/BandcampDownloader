using System;
using System.Globalization;
using System.Windows.Data;

namespace BandcampDownloader.UI.Converters;

/// <summary>
/// Converts Boolean values to their logical AND operation result.
/// </summary>
internal sealed class BooleanAndConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        foreach (var value in values)
        {
            if (value is bool boolean && !boolean)
            {
                return false;
            }
        }

        return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return null;
    }
}
