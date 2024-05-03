using System;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;

namespace CrossPlatformUI.ViewModels;

public class FlagsViewModel : ReactiveValidationObject
{

    
    public FlagsViewModel(MainViewModel mainViewModel)
    {
        config = mainViewModel.Config;
    }

    public string Flags
    {
        get => config.Serialize();
        set => this.RaiseAndSetIfChanged(ref config, new RandomizerConfiguration(value));
    }

    private RandomizerConfiguration config;

    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
}