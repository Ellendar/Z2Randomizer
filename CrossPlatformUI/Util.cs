using System;
using System.Globalization;
using Avalonia.Data.Converters;
using RandomizerCore;

namespace CrossPlatformUI;

public static class Util
{
    public static IValueConverter EnumConvert { get; } = new EnumDescriptionConverter();
}
public class EnumDescriptionConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value?.GetType().IsEnum != null)
        {
            return ((Enum)value).ToDescription();
        }
        return targetType.ToDefault();
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EnumDescription enumDescription)
        {
            return enumDescription.Value!;
        }
        return targetType.ToDefault();
    }
}