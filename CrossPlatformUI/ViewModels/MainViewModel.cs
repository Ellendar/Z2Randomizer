using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.ViewModels;

[RequiresUnreferencedCode("")]
public class MainViewModel : ReactiveValidationObject, IScreen, IActivatableViewModel
{
    public string? OutputFilePath { get; set; }
    private RandomizerConfiguration config = new();
    /// Useful inexpensive shared observable for views to attach onto
    /// for chaining change detection logic
    public IObservable<Unit> FlagsChanged { get; }

    public IObservable<String> FlagsObservable { get; }

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
        FlagsChanged = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
            h => Config.PropertyChanged += h,
            h => Config.PropertyChanged -= h)
            .Where(e => e.EventArgs.PropertyName == "Flags")
            .Select(_ => Unit.Default)
            .Replay(1)
            .RefCount();

        FlagsObservable = FlagsChanged
            .Select(_ => this.Config.SerializeFlags())
            .DistinctUntilChanged()
            .Replay(1)
            .RefCount();

        RomFileViewModel = new(this);
        GenerateRomViewModel = new(this);
        SaveNewPresetViewModel = new(this);
        RandomizerViewModel = new(this);
        Router.Navigate.Execute(RandomizerViewModel);

        GenerateRom = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute()
        );

        this.WhenActivated((CompositeDisposable disposables) =>
        {
            if (!RomFileViewModel.HasRomData)
            {
                Router.Navigate.Execute(RomFileViewModel);
            }
        });
    }

    // Window/Desktop specific data
    
    private const int DefaultWidth = 940;
    private const int DefaultHeight = 820;
    
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
