
using System;
using System.Runtime.CompilerServices;
using Avalonia.Data;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels;

public class ViewModelBase : ReactiveObject { }

public static class Extension {
    public static void ValueOrException<TParent, TBacking>(this TParent parent, ref TBacking backing,
        Func<TBacking> newValue, string? message = null, [CallerMemberName] string? field = "")
        where TParent : IReactiveObject
    {
        try
        {
            parent.RaiseAndSetIfChanged(ref backing, newValue.Invoke(), field);
        }
        catch (Exception e)
        {
            throw new DataValidationException(message ?? e.InnerException?.Message ?? "Invalid!");
        }
    }
}