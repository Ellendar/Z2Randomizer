using Avalonia.Data.Converters;
using Avalonia.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;

namespace CrossPlatformUI.Converters;

public class SumOfValuesLessThanConverter : IMultiValueConverter
{
    public static readonly SumOfValuesLessThanConverter Instance = new();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if(values.Any(i => i is UnsetValueType))
        {
            return BindingOperations.DoNothing;
        }
        if (values.Any(i => i == null))
        {
            return BindingOperations.DoNothing;
        }
 
        int sum = 0;
        foreach (object? value in values)
        {
            if(!int.TryParse(value!.ToString(), out int intVal))
            {
                return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
            }
            
            sum += intVal!;
        }
        if (!int.TryParse(parameter!.ToString(), out int paramVal))
        {
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }
        return sum < paramVal;

    }
}
