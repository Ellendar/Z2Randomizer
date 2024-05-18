using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.Json.Serialization;
using RandomizerCore;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace CrossPlatformUI.ViewModels;

public class MainViewModel : ReactiveValidationObject, IScreen, IActivatableViewModel
{
    public const string BeginnerPreset = "RAhN6FAABRFJAAAJAJBcZB+3mCAqBAFjALBJWA";
    public const string StandardPreset = "Ao!V2thCqLsVAAAAFThAAg!AFVVhFVnAAAALhA";
    public const string MaxRandoPreset = "Go!V2thCqLsVAAAAFThAAg!AFVVhFVnAAAALhA";
    public const string RandoPercentPreset = "Zo!V2thCqLsVAAAAFThAAg!AFVVhFVnAAAALhA";
    
    public string? OutputFilePath { get; set; }

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public RandomizerConfiguration Config { get; set; } = new();

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public RomFileViewModel RomFileViewModel { get; set; }
    
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public RandomizerViewModel RandomizerViewModel { get; set; }

    public CustomPixelPoint WindowPosition { get => windowPosition; set => this.RaiseAndSetIfChanged(ref windowPosition, value); }

    public CustomSize WindowSize { get => windowSize; set => this.RaiseAndSetIfChanged(ref windowSize, value); }

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
    
    // Window/Desktop specific data
    
    private const int DefaultWidth = 900;
    private const int DefaultHeight = 650;
    
    private CustomPixelPoint windowPosition = new()
    {
        X = 0,
        Y = 0
    };
    private CustomSize windowSize = new()
    {
        Width = DefaultWidth,
        Height = DefaultHeight
    };
    // public void OnDeserializing()
    // {
    //     App.Main = this;
    // }

    // The Router associated with this Screen.
    // Required by the IScreen interface.
    [JsonIgnore]
    public RoutingState Router { get; } = new ();

    // The command that navigates a user to first view model.
    [JsonIgnore]
    public ReactiveCommand<Unit, IRoutableViewModel> GenerateRom { get; }
    
    [JsonIgnore]
    public GenerateRomViewModel GenerateRomViewModel { get; }
    
    // Unique identifier for the routable view model.
    [JsonIgnore]
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    [JsonIgnore]
    public ViewModelActivator Activator { get; } = new ();
}


public class CustomPixelPoint
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class CustomSize
{
    public double Width { get; set; }
    public double Height { get; set; }
}
