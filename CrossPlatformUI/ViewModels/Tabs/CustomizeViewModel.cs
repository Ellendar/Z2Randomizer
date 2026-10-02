using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using ReactiveUI;
using ReactiveUI.Primitives;

namespace CrossPlatformUI.ViewModels.Tabs;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class CustomizeViewModel : ReactiveObject
{
    public SpritePreviewViewModel SpritePreviewViewModel { get; }

    public IObservable<bool> RandomizeMusicEnabledObservable { get; }
    public IObservable<bool> MixCustomAndOriginalMusicEnabledObservable { get; }
    public IObservable<bool> IncludeDiverseMusicEnabledObservable { get; }
    public IObservable<bool> DisableUnsafeMusicEnabledObservable { get; }

    [JsonConstructor]
#pragma warning disable CS8618 
    public CustomizeViewModel() {}
#pragma warning restore CS8618 
    public CustomizeViewModel(MainViewModel main)
    {
        Main = main;
        SpritePreviewViewModel = new(main);

        RandomizeMusicEnabledObservable = Main.Config
            .WhenAnyValue(c => c.DisableMusic)
            .Select(customMusicEnabled => !customMusicEnabled)
            .DistinctUntilChanged();

        var musicSuboptionsEnabled = Main.Config
            .WhenAnyValue(c => c.DisableMusic, c => c.RandomizeMusic)
            .Select(music => !music.Property1 && music.Property2)
            .DistinctUntilChanged()
            .Replay(1).RefCount();
        MixCustomAndOriginalMusicEnabledObservable = musicSuboptionsEnabled;
        IncludeDiverseMusicEnabledObservable = musicSuboptionsEnabled;
        DisableUnsafeMusicEnabledObservable = musicSuboptionsEnabled;
    }

    [JsonIgnore]
    public MainViewModel Main { get; }
}
