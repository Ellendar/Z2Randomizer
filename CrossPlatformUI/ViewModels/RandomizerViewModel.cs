using System;
using System.Reactive;
using System.Runtime.Serialization;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;

namespace CrossPlatformUI.ViewModels;

[DataContract]
public class RandomizerViewModel : ReactiveValidationObject, IRoutableViewModel
{
    public RandomizerViewModel(MainViewModel mainViewModel)
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
