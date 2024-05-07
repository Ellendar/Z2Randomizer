using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Runtime.Serialization;
using System.Threading;
using CrossPlatformUI.Services;
using DialogHostAvalonia;
using RandomizerCore.Asm;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.Core;
using Microsoft.Extensions.DependencyInjection;
using Z2Randomizer.Core.Sidescroll;

namespace CrossPlatformUI.ViewModels;

public class GenerateRomViewModel : ReactiveValidationObject, IRoutableViewModel, IActivatableViewModel
{

    private string progress = "";
    public string Progress {
        get => string.IsNullOrEmpty(progress) ? "Starting Seed Generation" : progress;
        set => this.RaiseAndSetIfChanged(ref progress, value);
    }
    
    public GenerateRomViewModel()
    {
        HostScreen = null;
        Activator = new();
    }
    
    public ReactiveCommand<Unit, Unit> CancelGeneration { get; }
    
    public GenerateRomViewModel(MainViewModel screen)
    {
        HostScreen = screen;
        Activator = new();
        var tokenSource = new CancellationTokenSource();
        CancelGeneration = ReactiveCommand.Create(() =>
        {
            tokenSource.Cancel();
            DialogHost.Close("GenerateRomDialog");
        });

        this.WhenActivated(Randomize);
        return;

        async void Randomize(CompositeDisposable disposables)
        {
            Disposable.Create(() => { tokenSource.Cancel(); })
                .DisposeWith(disposables);
            await App.PersistState();
            // var engine = App.Current?.Services?.GetService<IAsmEngine>();
            // var roomsJson = await fileService!.OpenFileAsync();
            // var customJson = config.UseCustomRooms ? await fileService!.OpenFileAsync() : null;
            // var palaceRooms = new PalaceRooms("", null);
            // var randomizer = new Hyrule(engine!, palaceRooms);
            // var host = (HostScreen as MainViewModel)!;
            // var output = await randomizer.Randomize(host.RomFileViewModel.RomData!, host.Config, str => Progress = str, tokenSource.Token);
        }
    }

    // Reference to IScreen that owns the routable view model.
    public IScreen HostScreen { get; }
    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    public ViewModelActivator Activator { get; }
    
}