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
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value?.GetType().IsEnum != null)
        {
            return ((Enum)value).ToDescription();
        }
        throw new ArgumentException("Convert:Value must be an enum.");
    }
    
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EnumDescription enumDescription)
        {
            return enumDescription.Value!;
        }
        throw new ArgumentException("ConvertBack:EnumDescription must be an enum.");
    }
}