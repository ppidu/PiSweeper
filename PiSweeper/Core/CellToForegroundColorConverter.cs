using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using PiSweeper.ViewModels;

namespace PiSweeper.Core;

public sealed class CellToForegroundColorConverter : IMultiValueConverter
{
    public object? Convert(IList<object?>? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values != null && values.Any(c => c is UnsetValueType)) return Brushes.Black;
        if (values is not [int val, bool isFlagged]) throw new NotSupportedException();
        if (isFlagged) return Brushes.Black;
        return val switch
        {
            -1 => Brushes.Black,
            0 => Brushes.Transparent,
            1 => Brushes.Blue,
            2 => Brushes.Green,
            3 => Brushes.Red,
            4 => Brushes.DarkBlue,
            5 => Brushes.Orange,
            6 => Brushes.Cyan,
            7 => Brushes.Black,
            8 => Brushes.DimGray,
            _ => throw new ArgumentOutOfRangeException(nameof(val), "Value must be between -1 and 8.")
        };
    }
}