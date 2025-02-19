using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.Json.Serialization;
using Avalonia.Controls;
using DialogHostAvalonia;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;

namespace CrossPlatformUI.ViewModels;

    
    
public class CustomPreset : ReactiveObject
{
    private string preset = "";
    public string Preset { get => preset; set => this.RaiseAndSetIfChanged(ref preset, value); }
    private string flags = "";
    public string Flags { get => flags; set => this.RaiseAndSetIfChanged(ref flags, value); }
}

public class SaveNewPresetViewModel : ReactiveValidationObject, IRoutableViewModel, IActivatableViewModel
{

#pragma warning disable CS8618
    [JsonConstructor]
    public SaveNewPresetViewModel() {}
#pragma warning restore CS8618
    
    public SaveNewPresetViewModel(MainViewModel main)
    {
        Main = main;
        HostScreen = Main;
        PresetName = string.Empty;
        Activator = new();
        SavePreset = ReactiveCommand.Create(() => {
            Main.SaveNewPresetDialogOpen = false;
            SavedPresets.Add(new CustomPreset{Preset = PresetName, Flags = Main.RandomizerViewModel.Flags});
        });
        CancelPreset = ReactiveCommand.Create(() =>
        {
            Main.SaveNewPresetDialogOpen = false;
        });
        this.WhenActivated((CompositeDisposable disposables) =>
        {
            PresetName = string.Empty;
        });
    }
    
    private ObservableCollection<CustomPreset> savedPresets = new ();
    public ObservableCollection<CustomPreset> SavedPresets { get => savedPresets; set => this.RaiseAndSetIfChanged(ref savedPresets, value); }

    private string presetName = "";
    [JsonIgnore]
    public string PresetName { get => presetName; set => this.RaiseAndSetIfChanged(ref presetName, value); }
    
    [JsonIgnore]
    private MainViewModel Main { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> SavePreset { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> CancelPreset { get; }


    [JsonIgnore]
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    [JsonIgnore]
    public IScreen HostScreen { get; }
    [JsonIgnore]
    public ViewModelActivator Activator { get; }
}