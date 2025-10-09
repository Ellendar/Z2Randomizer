using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.Json.Serialization;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.ViewModels;

public class MainViewModel : ReactiveValidationObject, IScreen, IActivatableViewModel
{
    public string? OutputFilePath { get; set; }
    private RandomizerConfiguration config = new();

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public RandomizerConfiguration Config { get => config; set => this.RaiseAndSetIfChanged(ref config, value); }

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public RomFileViewModel RomFileViewModel { get; set; }
    
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public RandomizerViewModel RandomizerViewModel { get; set; }

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public SaveNewPresetViewModel SaveNewPresetViewModel { get; set; }

    public CustomPixelPoint WindowPosition { get => windowPosition; set => this.RaiseAndSetIfChanged(ref windowPosition, value); }

    public CustomSize WindowSize { get => windowSize; set => this.RaiseAndSetIfChanged(ref windowSize, value); }

    public MainViewModel()
    {
        RomFileViewModel = new(this);
        GenerateRomViewModel = new(this);
        SaveNewPresetViewModel = new(this);
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

    private bool shuffleAllExp;
    [JsonIgnore]
    public bool ShuffleAllExpState
    {
        get => shuffleAllExp;
        set => this.RaiseAndSetIfChanged(ref shuffleAllExp, value);
    }
    
    // The Router associated with this Screen.
    // Required by the IScreen interface.
    [JsonIgnore]
    public RoutingState Router { get; } = new ();

    // The command that navigates a user to first view model.
    [JsonIgnore]
    public ReactiveCommand<Unit, IRoutableViewModel> GenerateRom { get; }
    
    [JsonIgnore]
    public GenerateRomViewModel GenerateRomViewModel { get; }

    private bool generateRomDialogOpen = false;
    [JsonIgnore]
    public bool GenerateRomDialogOpen { get => generateRomDialogOpen; set => this.RaiseAndSetIfChanged(ref generateRomDialogOpen, value); }


    private bool saveNewPresetDialogOpen = false;
    [JsonIgnore]
    public bool SaveNewPresetDialogOpen { get => saveNewPresetDialogOpen; set => this.RaiseAndSetIfChanged(ref saveNewPresetDialogOpen, value); }

    
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
