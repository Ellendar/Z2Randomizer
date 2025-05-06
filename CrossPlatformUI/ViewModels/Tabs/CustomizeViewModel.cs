using System.Reactive.Linq;
using System.Text.Json.Serialization;
using ReactiveUI;
using Z2Randomizer.RandomizerCore;

namespace CrossPlatformUI.ViewModels.Tabs;

public class CustomizeViewModel : ReactiveObject
{
    public SpritePreviewViewModel SpritePreviewViewModel { get; }

    [JsonConstructor]
#pragma warning disable CS8618 
    public CustomizeViewModel() {}
#pragma warning restore CS8618 
    public CustomizeViewModel(MainViewModel main)
    {
        Main = main;
        SpritePreviewViewModel = new(main);

        _randomizeMusicEnabled = Main.Config
            .WhenAnyValue(c => c.DisableMusic, c => c.RandomizeMusic)
            .Select(tuple => !tuple.Item1 && tuple.Item2)
            .ToProperty(this, t => t.RandomizeMusicEnabled);
    }

    public bool DisableMusic
    {
        get => Main.Config.DisableMusic;
        set
        {
            Main.Config.DisableMusic = value;
            this.RaisePropertyChanged();
        }
    }
    public bool RandomizeMusic
    {
        get => Main.Config.RandomizeMusic;
        set
        {
            Main.Config.RandomizeMusic = value;
            this.RaisePropertyChanged();
        }
    }
    private readonly ObservableAsPropertyHelper<bool> _randomizeMusicEnabled;
    public bool RandomizeMusicEnabled => _randomizeMusicEnabled.Value;
    public bool MixCustomAndOriginalMusic
    {
        get => Main.Config.MixCustomAndOriginalMusic;
        set
        {
            Main.Config.MixCustomAndOriginalMusic = value;
            this.RaisePropertyChanged();
        }
    }
    public bool DisableUnsafeMusic
    {
        get => Main.Config.DisableUnsafeMusic;
        set
        {
            Main.Config.DisableUnsafeMusic = value;
            this.RaisePropertyChanged();
        }
    }
    public bool FastSpellCasting
    {
        get => Main.Config.FastSpellCasting;
        set
        {
            Main.Config.FastSpellCasting = value;
            this.RaisePropertyChanged();
        }
    }
    public bool UpAOnController1
    {
        get => Main.Config.UpAOnController1;
        set
        {
            Main.Config.UpAOnController1 = value;
            this.RaisePropertyChanged();
        }
    }

    public bool RemoveFlashing
    {
        get => Main.Config.RemoveFlashing;
        set
        {
            Main.Config.RemoveFlashing = value;
            this.RaisePropertyChanged();
        }
    }

    public bool ShuffleSpritePalettes
    {
        get => Main.Config.ShuffleSpritePalettes;
        set
        {
            Main.Config.ShuffleSpritePalettes = value;
            this.RaisePropertyChanged();
        }
    }
    public bool UseCommunityText
    {
        get => Main.Config.UseCommunityText;
        set
        {
            Main.Config.UseCommunityText = value;
            this.RaisePropertyChanged();
        }
    }

    public BeepThreshold BeepThreshold
    {
        get => Main.Config.BeepThreshold;
        set
        {
            Main.Config.BeepThreshold = value;
            this.RaisePropertyChanged();
        }
    }

    public BeepFrequency BeepFrequency
    {
        get => Main.Config.BeepFrequency;
        set
        {
            Main.Config.BeepFrequency = value;
            this.RaisePropertyChanged();
        }
    }

    public bool DisableHUDLag
    {
        get => Main.Config.DisableHUDLag;
        set
        {
            Main.Config.DisableHUDLag = value;
            this.RaisePropertyChanged();
        }
    }

    [JsonIgnore]
    public MainViewModel Main { get; }
}