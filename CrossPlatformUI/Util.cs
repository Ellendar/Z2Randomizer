using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Serialization;
using Avalonia.Data.Converters;
using RandomizerCore;
using RandomizerCore.Flags;

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