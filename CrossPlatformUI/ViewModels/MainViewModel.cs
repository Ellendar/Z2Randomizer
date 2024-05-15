using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.Serialization;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Contexts;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;

namespace CrossPlatformUI.ViewModels;

[DataContract]
public class MainViewModel : ReactiveValidationObject, IScreen, IActivatableViewModel
{
    public static readonly string BeginnerPreset = "RAhN6FAABRFJAAAJAJBcZB+3mCAqBAFjALBJWA";
    public static readonly string StandardPreset = "Ao!V2thCqLsVAAAAFThAAg!AFVVhFVnAAAALhA";
    public static readonly string MaxRandoPreset = "Go!V2thCqLsVAAAAFThAAg!AFVVhFVnAAAALhA";
    public static readonly string RandoPercentPreset = "Zo!V2thCqLsVAAAAFThAAg!AFVVhFVnAAAALhA";
    
    public RandomizerConfiguration Config { get; } = new();

    // The Router associated with this Screen.
    // Required by the IScreen interface.
    public RoutingState Router { get; } = new ();

    // The command that navigates a user to first view model.
    public ReactiveCommand<Unit, IRoutableViewModel> GenerateRom { get; }
    
    [DataMember]
    public string OutputFilePath { get; set; }

    [DataMember]
    public RomFileViewModel RomFileViewModel { get; }
    [DataMember]
    public RandomizerViewModel RandomizerViewModel { get; }
    public GenerateRomViewModel GenerateRomViewModel { get; }

    
    public MainViewModel()
    {
        RomFileViewModel = new(this);
        GenerateRomViewModel = new(this);
        RandomizerViewModel = new(this);
        Router.Navigate.Execute(RandomizerViewModel);

        GenerateRom = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute()
        );
        
        this.WhenActivated(ShowRomFileViewIfNoRom);
        return;
        void ShowRomFileViewIfNoRom(CompositeDisposable disposables)
        {
            if (!RomFileViewModel.HasRomData)
            {
                Router.Navigate.Execute(RomFileViewModel);
            }
            Disposable.Create(() => { })
                .DisposeWith(disposables);
        }
    }
    
    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public ViewModelActivator Activator { get; } = new ();
}