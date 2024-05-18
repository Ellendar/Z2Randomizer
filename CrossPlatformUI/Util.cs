using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using Avalonia.Data.Converters;
using RandomizerCore;
using RandomizerCore.Flags;
using RandomizerCore.Overworld;

namespace CrossPlatformUI;

public static class Util
{
    public static IValueConverter EnumConvert { get; } = new EnumDescriptionConverter();
    public static IValueConverter ClimateConvert { get; } = new ClimateConverter();
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
public class ClimateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.GetType() == typeof(Climate) ? ((Climate)value).Name : null;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is string climate ? Climates.ClimateList.FirstOrDefault(c => c == climate, null) : null;
    }
}