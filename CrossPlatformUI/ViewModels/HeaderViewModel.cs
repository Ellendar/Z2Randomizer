using System;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Data;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;

namespace CrossPlatformUI.ViewModels;

[DataContract]
public class HeaderViewModel : ReactiveValidationObject, IRoutableViewModel
{
    public HeaderViewModel(MainViewModel mainViewModel)
    {
        config = mainViewModel.Config;
        HostScreen = mainViewModel;
        
        RerollSeed = ReactiveCommand.Create(() =>
        {
            Seed = new Random().Next(0, 999999999).ToString();
        });
    }

    public ReactiveCommand<Unit, Unit> RerollSeed { get; }

    [DataMember]
    public string? Flags
    {
        get => config.Serialize();
        set => this.ValueOrException(ref config, () => new RandomizerConfiguration(value), "Invalid Flags");
    }
    
    [DataMember]
    public string Seed
    {
        get => seed;
        set => this.ValueOrException(ref seed, () => value, "Invalid Seed");
    }

    private RandomizerConfiguration config;
    private string seed = "";
    
    [IgnoreDataMember]
    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    [IgnoreDataMember]
    public IScreen HostScreen { get; }
}

internal static class Extension {
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