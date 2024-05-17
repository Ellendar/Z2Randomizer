using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using RandomizerCore;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels.Tabs;

public class CustomizeViewModel : ReactiveObject
{
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public SpritePreviewViewModel SpritePreviewViewModel { get; }

    [JsonConstructor]
    public CustomizeViewModel() {}
    public CustomizeViewModel(MainViewModel main)
    {
        Main = main;
        SpritePreviewViewModel = new(main);
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
    
    [JsonIgnore]
    public MainViewModel Main { get; }
}