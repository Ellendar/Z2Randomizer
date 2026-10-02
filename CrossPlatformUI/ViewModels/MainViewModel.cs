using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using ReactiveUI;
using RUISG = ReactiveUI.SourceGenerators;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Disposables;
using ReactiveUI.Primitives.Signals;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.ViewModels;

[RequiresUnreferencedCode("")]
public partial class MainViewModel : ReactiveObject, IScreen, IActivatableViewModel
{
    public string? OutputFilePath { get; set; }
    private readonly RandomizerConfiguration config = new();
    /// Useful inexpensive shared observable for views to attach onto
    /// for chaining change detection logic
    public IObservable<RxVoid> FlagsChanged { get; }

    public IObservable<String> FlagsObservable { get; }

    public IObservable<bool> ShuffleAttackExperienceEnabledObservable { get; }
    public IObservable<bool> ShuffleMagicExperienceEnabledObservable { get; }
    public IObservable<bool> ShuffleLifeExperienceEnabledObservable { get; }

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    [RUISG.Reactive]
    public partial RandomizerConfiguration Config { get; set; } = new();

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public RomFileViewModel RomFileViewModel { get; set; }
    
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public RandomizerViewModel RandomizerViewModel { get; set; }

    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public SaveNewPresetViewModel SaveNewPresetViewModel { get; set; }

    [RUISG.Reactive]
    public partial CustomPixelPoint WindowPosition { get; set; }

    [RUISG.Reactive]
    public partial CustomSize WindowSize { get; set; }

    public MainViewModel()
    {
        FlagsChanged = Signal.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
            h => Config.PropertyChanged += h,
            h => Config.PropertyChanged -= h)
            .Where(e => e.EventArgs.PropertyName == "Flags")
            .Select(_ => default(RxVoid))
            .StartWith(default(RxVoid))
            .Replay(1)
            .RefCount();

        FlagsObservable = FlagsChanged
            .Select(_ => this.Config.SerializeFlags())
            .DistinctUntilChanged()
            .Replay(1)
            .RefCount();

        var anyExperienceShuffle = this.WhenAnyValue(x => x.ShuffleAllExpState)
            .Select(shuffleAll => !shuffleAll)
            .DistinctUntilChanged()
            .Replay(1).RefCount();

        ShuffleAttackExperienceEnabledObservable = anyExperienceShuffle;
        ShuffleMagicExperienceEnabledObservable = anyExperienceShuffle;
        ShuffleLifeExperienceEnabledObservable = anyExperienceShuffle;

        RomFileViewModel = new(this);
        GenerateRomViewModel = new(this);
        SaveNewPresetViewModel = new(this);
        RemovePresetViewModel = new(this);
        UpdatePresetViewModel = new(this);
        RandomizerViewModel = new(this);
        SubscribeExtensions.Subscribe(Router.Navigate.Execute(RandomizerViewModel));

        GenerateRom = ReactiveCommand.CreateFromObservable(
            () => Router.Navigate.Execute()
        );

        this.WhenActivated((MultipleDisposable disposables) =>
        {
            if (!RomFileViewModel.HasRomData)
            {
                SubscribeExtensions.Subscribe(Router.Navigate.Execute(RomFileViewModel));
            }
        });
    }

    // Window/Desktop specific data
    
    private const int DefaultWidth = 900;
    private const int DefaultHeight = 750;
    
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

    [JsonIgnore]
    [RUISG.Reactive]
    public partial bool ShuffleAllExpState { get; set; }
    
    // The Router associated with this Screen.
    // Required by the IScreen interface.
    [JsonIgnore]
    public RoutingState Router { get; } = new ();

    // The command that navigates a user to first view model.
    [JsonIgnore]
    public ReactiveCommand<RxVoid, IRoutableViewModel> GenerateRom { get; }
    
    [JsonIgnore]
    public GenerateRomViewModel GenerateRomViewModel { get; }

    [JsonIgnore]
    [RUISG.Reactive]
    public partial bool GenerateRomDialogOpen { get; set; }


    [JsonIgnore]
    [RUISG.Reactive]
    public partial bool SaveNewPresetDialogOpen { get; set; }

    [JsonIgnore]
    public UpdatePresetViewModel UpdatePresetViewModel { get; }

    [JsonIgnore]
    [RUISG.Reactive]
    public partial bool UpdatePresetDialogOpen { get; set; }

    [JsonIgnore]
    public RemovePresetViewModel RemovePresetViewModel { get; }

    [JsonIgnore]
    [RUISG.Reactive]
    public partial bool RemovePresetDialogOpen { get; set; }

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
