using System;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;

namespace CrossPlatformUI.ViewModels;

public class MainViewModel : ReactiveValidationObject, IRoutableViewModel
{

    private RandomizerConfiguration config;
    public RandomizerConfiguration Config { get => config; set => this.RaiseAndSetIfChanged(ref config, value); }

    public string Flags
    {
        get => config.Serialize();
        set => this.RaiseAndSetIfChanged(ref config, new RandomizerConfiguration(value));
    }
    
    public MainViewModel()
    {
        // For the viewer
    }
    
    public MainViewModel(IScreen screen)
    {
        HostScreen = screen;
        config = new();
        this.ValidationRule(
            viewModel => viewModel.config, 
            cfg => !string.IsNullOrWhiteSpace(cfg?.Serialize()),
            "Flags Invalid.");
    }
    
    // Reference to IScreen that owns the routable view model.
    public IScreen HostScreen { get; }
    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
}