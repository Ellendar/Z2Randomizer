using System;
using Avalonia.Data;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;

namespace CrossPlatformUI.ViewModels;

public class HeaderViewModel : ReactiveValidationObject, IRoutableViewModel
{
    public HeaderViewModel(MainViewModel mainViewModel)
    {
        config = mainViewModel.Config;
        HostScreen = mainViewModel;
    }

    public string Flags
    {
        get => config.Serialize();
        set => this.ValueOrException(ref config, () => new RandomizerConfiguration(value), "Invalid Flags");
    }
    
    public string Seed
    {
        get => seed;
        set => this.ValueOrException(ref seed, () => value, "Invalid Seed");
    }

    private RandomizerConfiguration config;
    private string seed;

    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    public IScreen HostScreen { get; }
    
}

internal static class Extension {
    public static void ValueOrException<TParent, TBacking>(this TParent parent, ref TBacking backing, Func<TBacking> newValue, string? message = null)
        where TParent : IReactiveObject
    {
        try
        {
            parent.RaiseAndSetIfChanged(ref backing, newValue.Invoke());
        }
        catch (Exception e)
        {
            throw new DataValidationException(message ?? e.InnerException?.Message ?? "Invalid!");
        }
    }
}