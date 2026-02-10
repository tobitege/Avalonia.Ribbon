using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace AvaloniaUI.Ribbon.Converters;

/// <summary>
///     This is a converter which will add two numbers
/// </summary>
public class MathAddConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        var value = values.Sum(x => ToDouble(x, culture) ?? 0d);
        return value + (ToDouble(parameter, culture) ?? 0d);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // If we want to convert back, we need to subtract instead of add.
        var left = ToDouble(value, culture);
        if (left == null)
            return null;

        return left - (ToDouble(parameter, culture) ?? 0d);
    }

    private static double? ToDouble(object? value, CultureInfo culture)
    {
        if (value == null)
            return null;

        if (value is double doubleValue)
            return doubleValue;

        if (value is IConvertible convertible)
        {
            try
            {
                return convertible.ToDouble(culture);
            }
            catch (FormatException)
            {
                return null;
            }
            catch (InvalidCastException)
            {
                return null;
            }
        }

        return null;
    }
}