using System;
using System.Reactive;
using System.Runtime.Serialization;
using Avalonia.Data;
using ReactiveUI;
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
        
        RerollSeed = ReactiveCommand.Create(() =>
        {
            mainViewModel.Config.Seed = new Random().Next(0, 999999999).ToString();
        });
        
        mainViewModel.Config.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case "Flags":
                    this.RaisePropertyChanged(nameof(Flags));
                    break;
                case "Seed":
                    this.RaisePropertyChanged(nameof(Seed));
                    break;
            }
        };
    }

    public MainViewModel Main { get; }
    public ReactiveCommand<Unit, Unit> RerollSeed { get; }

    private bool isFlagsValid;
    public bool IsFlagsValid { get => isFlagsValid; set => this.RaiseAndSetIfChanged(ref isFlagsValid, value); }
    
    [DataMember]
    public string Flags
    {
        get => Main.Config.Flags;
        set
        {
            try
            {
                // Setting this flags like this will both validate the flag string
                // and also notify all observers for the individual options and the flag string itself
                Main.Config.Flags = new RandomizerConfiguration(value).Flags;
                this.RaisePropertyChanged();
                IsFlagsValid = true;
            }
            catch
            {
                throw new DataValidationException("Invalid Flags");
                IsFlagsValid = false;
            }
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
