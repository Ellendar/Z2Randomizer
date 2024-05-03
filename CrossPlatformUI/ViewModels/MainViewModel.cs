using System;
using System.Reactive;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;

namespace CrossPlatformUI.ViewModels;

public class MainViewModel : ReactiveValidationObject, IScreen
{

    private RandomizerConfiguration config;
    public RandomizerConfiguration Config { get => config; set => this.RaiseAndSetIfChanged(ref config, value); }

    // The Router associated with this Screen.
    // Required by the IScreen interface.
    public RoutingState Router { get; } = new ();

    // The command that navigates a user to first view model.
    public ReactiveCommand<Unit, IRoutableViewModel> LoadRom { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> GenerateRom { get; }

    // The command that navigates a user back.
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack!;

    public RomFileViewModel RomFileViewModel { get; }

    
    public MainViewModel()
    {
        RomFileViewModel = new(this);
        if (!RomFileViewModel.HasRomData)
        {
            Router.Navigate.Execute(RomFileViewModel);
        }
        
        // GoNext = ReactiveCommand.CreateFromObservable(
        //     () => Router.Navigate.Execute(new MainViewModel(this))
        // );

        LoadRom = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(RomFileViewModel)
        );
        GenerateRom = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute(new GenerateRomViewModel(this, Config, RomFileViewModel.RomData))
        );
        
        config = new();
        this.ValidationRule(
            viewModel => viewModel.config, 
            cfg => !string.IsNullOrWhiteSpace(cfg?.Serialize()),
            "Flags Invalid.");
    }
    
    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
}