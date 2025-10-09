using System;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Overworld;

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
        if (value is string climate) 
        {
            return Climates.ClimateList.FirstOrDefault(c => c.Name == climate);
        }
        return null;
            
    }
}