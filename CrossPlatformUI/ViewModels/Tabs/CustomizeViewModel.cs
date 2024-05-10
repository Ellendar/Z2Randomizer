using System.Runtime.Serialization;
using RandomizerCore;
using ReactiveUI;

namespace CrossPlatformUI.ViewModels.Tabs;

[DataContract]
public class CustomizeViewModel(MainViewModel main) : ReactiveObject
{
    public MainViewModel Main { get; } = main;
    
    [DataMember]
    public SpritePreviewViewModel SpritePreviewViewModel { get; } = new (main);
    
    [DataMember]
    public bool DisableMusic
    {
        get => Main.Config.DisableMusic;
        set
        {
            Main.Config.DisableMusic = value;
            this.RaisePropertyChanged();
        }
    }
    
    [DataMember]
    public bool FastSpellCasting
    {
        get => Main.Config.FastSpellCasting;
        set
        {
            Main.Config.FastSpellCasting = value;
            this.RaisePropertyChanged();
        }
    }
    
    [DataMember]
    public bool UpAOnController1
    {
        get => Main.Config.UpAOnController1;
        set
        {
            Main.Config.UpAOnController1 = value;
            this.RaisePropertyChanged();
        }
    }
    
    [DataMember]
    public bool RemoveFlashing
    {
        get => Main.Config.RemoveFlashing;
        set
        {
            Main.Config.RemoveFlashing = value;
            this.RaisePropertyChanged();
        }
    }
    
    [DataMember]
    public bool ShuffleSpritePalettes
    {
        get => Main.Config.ShuffleSpritePalettes;
        set
        {
            Main.Config.ShuffleSpritePalettes = value;
            this.RaisePropertyChanged();
        }
    }
    
    [DataMember]
    public BeepThreshold BeepThreshold
    {
        get => Main.Config.BeepThreshold;
        set
        {
            Main.Config.BeepThreshold = value;
            this.RaisePropertyChanged();
        }
    }
    
    [DataMember]
    public BeepFrequency BeepFrequency
    {
        get => Main.Config.BeepFrequency;
        set
        {
            Main.Config.BeepFrequency = value;
            this.RaisePropertyChanged();
        }
    }
}