using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
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
    
    public ReactiveCommand<Unit, Unit> CancelGeneration { get; }
    
    private MainViewModel Main { get; }

    private CancellationTokenSource? tokenSource;
    
    public GenerateRomViewModel(MainViewModel screen)
    {
        HostScreen = screen;
        Main = screen;
        Activator = new();
        CancelGeneration = ReactiveCommand.Create(() =>
        {
            tokenSource?.Cancel();
            DialogHost.Close("GenerateRomDialog");
        });

        this.WhenActivated(Randomize);
        return;

        async void Randomize(CompositeDisposable disposables)
        {
            Disposable.Create(() => tokenSource?.Cancel())
                .DisposeWith(disposables);
            tokenSource = new CancellationTokenSource();
            Progress = "";
            await App.PersistState();
            var engine = App.Current?.Services?.GetService<IAsmEngine>();
            var files = App.Current?.Services?.GetService<IFileSystemService>();
            var host = (HostScreen as MainViewModel)!;
            var config = host.Config;
            var roomsJson = await files!.OpenFile(IFileSystemService.RandomizerPath.Palaces, "PalaceRooms.json");
            var customJson = config.UseCustomRooms ? await files.OpenFile(IFileSystemService.RandomizerPath.Palaces, "CustomRooms.json") : null;
            var rooms = config.UseCustomRooms ? customJson : roomsJson;
            var palaceRooms = new PalaceRooms(rooms!, config.UseCustomRooms);
            var randomizer = new Hyrule(engine!, palaceRooms);
            // Make a copy of the rom data to prevent seed bleed!
            var romdata = host.RomFileViewModel.RomData!.ToArray();
            var output = await randomizer.Randomize(romdata, config, str => Progress = str, tokenSource.Token);
            var filename = $"Z2_{config.Seed}_{config.Flags}.nes";
            await files.SaveGeneratedBinaryFile(filename, output!, Main.OutputFilePath);
            Progress = $"Generation Complete!\n\nFile {filename} created";
        }
    }

    // Reference to IScreen that owns the routable view model.
    public IScreen HostScreen { get; }
    // Unique identifier for the routable view model.
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    public ViewModelActivator Activator { get; }
    
}