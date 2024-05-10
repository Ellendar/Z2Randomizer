using System;
using System.Reactive;
using System.Runtime.Serialization;
using Avalonia.Data;
using CrossPlatformUI.ViewModels.Tabs;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;

namespace CrossPlatformUI.ViewModels;

[DataContract]
public class RandomizerViewModel : ReactiveValidationObject, IRoutableViewModel
{
    public RandomizerViewModel(MainViewModel mainViewModel)
    {
        HostScreen = mainViewModel;
        Main = mainViewModel;
        CustomizeViewModel = new(mainViewModel);
        Flags = MainViewModel.BeginnerPreset;
        
        RerollSeed = ReactiveCommand.Create(() =>
        {
            mainViewModel.Config.Seed = new Random().Next(0, 999999999).ToString();
        });
        
        LoadPreset = ReactiveCommand.Create<string>((flags) =>
        {
            mainViewModel.Config.Flags = flags;
        });
        
        LoadRom = ReactiveCommand.CreateFromObservable(
            () => mainViewModel.Router.Navigate.Execute(mainViewModel.RomFileViewModel)
        );
        mainViewModel.Config.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case "Flags":
                    Flags = ((RandomizerConfiguration)sender!).Flags;
                    break;
                case "Seed":
                    this.RaisePropertyChanged(nameof(Seed));
                    break;
            }
        };
        mainViewModel.RomFileViewModel.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case "HasRomData":
                    this.RaisePropertyChanged(nameof(CanGenerate));
                    break;
            }
        };
        var flagsValidation = this.WhenAnyValue(
            x => x.Flags,
            IsFlagStringValid
            );
        this.ValidationRule(
            x => x.Flags,
            flagsValidation,
            "Invalid Flags");
        CanGenerate = this.WhenAnyValue(
            x => x.Flags,
            x => x.Seed,
            x => x.Main.RomFileViewModel.HasRomData, 
            (flags, seed, hasRomData) => IsFlagStringValid(flags) && !string.IsNullOrWhiteSpace(seed) && hasRomData);
    }

    public MainViewModel Main { get; }
    public CustomizeViewModel CustomizeViewModel { get; }
    public ReactiveCommand<Unit, Unit> RerollSeed { get; }
    
    public ReactiveCommand<string, Unit> LoadPreset { get; }
    public ReactiveCommand<Unit, IRoutableViewModel> LoadRom { get; }

    public IObservable<bool> CanGenerate { get; }

    private bool IsFlagStringValid(string flags)
    {
        try
        {
            _ = new RandomizerConfiguration(flags);
            return true;
        }
        catch
        {
            return false;
        }
    }
    private string validatedFlags = "";
    [DataMember]
    public string Flags
    {
        get => validatedFlags;
        set
        {
            if (IsFlagStringValid(value) && value != Main.Config.Flags)
            {
                Main.Config.Flags = value;
            }
            this.RaiseAndSetIfChanged(ref validatedFlags, value);
            // try
            // {
            //     // Setting this flags like this will both validate the flag string
            //     // and also notify all observers for the individual options and the flag string itself
            //     Main.Config.Flags = new RandomizerConfiguration(value).Flags;
            //     this.RaiseAndSetIfChanged(ref validationFlags, )
            // }
            // catch
            // {
            //     
            // } 
            // finally
            // { 
            //     this.RaisePropertyChanged();
            //     this.RaisePropertyChanged(nameof(CanGenerate));
            // }
        }
    }
    
    [DataMember]
    public string Seed
    {
        get => Main.Config.Seed;
        set
        {
            Main.Config.Seed = value;
            this.RaisePropertyChanged();
        }
    }

    [IgnoreDataMember]
    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    [IgnoreDataMember]
    public IScreen HostScreen { get; }
}
