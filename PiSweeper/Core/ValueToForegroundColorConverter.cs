using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PiSweeper.Core;

public sealed class ValueToForegroundColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int val) return null;
        return val switch
        {
            -1 => Brushes.Transparent,
            0 => Brushes.Transparent,
            1 => Brushes.Blue,
            2 => Brushes.Green,
            3 => Brushes.Red,
            4 => Brushes.DarkBlue,
            5 => Brushes.Orange,
            6 => Brushes.Cyan,
            7 => Brushes.Black,
            8 => Brushes.DimGray,
            _ => throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 8.")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}