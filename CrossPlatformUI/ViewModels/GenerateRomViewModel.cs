using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CrossPlatformUI.Services;
using DialogHostAvalonia;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Microsoft.Extensions.DependencyInjection;
using RandomizerCore;
using RandomizerCore.Sidescroll;

namespace CrossPlatformUI.ViewModels;

public class GenerateRomViewModel : ReactiveValidationObject, IRoutableViewModel, IActivatableViewModel
{

#pragma warning disable CS8618

    [JsonConstructor]
    public GenerateRomViewModel() {}
#pragma warning restore CS8618 
    public GenerateRomViewModel(MainViewModel main)
    {
        Main = main;
        HostScreen = Main;
        HasError = false;
        Activator = new();
        CancelGeneration = ReactiveCommand.Create(() =>
        {
            tokenSource?.Cancel();
            Main.GenerateRomDialogOpen = false;
        });
        CopyError = ReactiveCommand.CreateFromTask(async () =>
        {
            var clipboard = App.TopLevel!.Clipboard!;
            var host = (HostScreen as MainViewModel)!;
            var config = host.Config;
            var version = Assembly.GetEntryAssembly()!.GetName().Version!;
            var versionstr = $"{version.Major}.{version.Minor}.{version.Build}";
            await clipboard.SetTextAsync($"""
Version: {versionstr}
Flags: {config.Flags}
Seed: {config.Seed}
```
{lastError}
```
""");
            Progress = "Error message copied to clipboard!";
        });

        this.WhenActivated(Randomize);
        return;

        async void Randomize(CompositeDisposable disposables)
        {
            if (!Main.GenerateRomDialogOpen) return;
            
            Disposable.Create(() => tokenSource?.Cancel())
                .DisposeWith(disposables);
            lastError = null;
            HasError = false;
            IsComplete = false;
            tokenSource = new CancellationTokenSource();
            Progress = "";
            await App.PersistState();
            var createAsm = App.Current?.Services?.GetService<Hyrule.NewAssemblerFn>();
            var files = App.Current?.Services?.GetService<IFileSystemService>();
            var host = (HostScreen as MainViewModel)!;
            var config = host.Config;
            var roomsJson = await files!.OpenFile(IFileSystemService.RandomizerPath.Palaces, "PalaceRooms.json");
            var customJson = config.UseCustomRooms ? await files.OpenFile(IFileSystemService.RandomizerPath.Palaces, "CustomRooms.json") : null;
            var rooms = config.UseCustomRooms ? customJson : roomsJson;
            var palaceRooms = new PalaceRooms(rooms!, config.UseCustomRooms);
            var randomizer = new Hyrule(createAsm, palaceRooms);
            Dispatcher.UIThread.Post(GenerateSeed, DispatcherPriority.Background);
            return;
            
            async void GenerateSeed()
            {
                try
                {
                    var romdata = host.RomFileViewModel.RomData!.ToArray();
                    var output = await randomizer.Randomize(romdata, config, UpdateProgress, tokenSource.Token);
                    var filename = $"Z2_{config.Seed}_{config.Flags}.nes";
                    await files.SaveGeneratedBinaryFile(filename, output!, Main.OutputFilePath);
                    if(config.GenerateSpoiler)
                    {
                        var spoilerFilename = $"Z2_{config.Seed}_{config.Flags}.spoiler";
                        await files.SaveSpoilerFile(spoilerFilename, randomizer.GenerateSpoiler(), Main.OutputFilePath);
                    }
                    Progress = $"Generation Complete! Hash: {randomizer.Hash}\n\nFile {filename} created";
                    IsComplete = true;
                }
                catch (Exception e)
                {
                    await tokenSource.CancelAsync();
                    lastError = e;
                    HasError = true;
                    await UpdateProgress($"Error generating seed! Please report this on the discord");
                }
            }
        }
    }

    private Task UpdateProgress(string str)
    {
        return Dispatcher.UIThread.InvokeAsync(() => Progress = str).GetTask();
    }

    private string progress = "";
    [JsonIgnore]
    public string Progress {
        get => string.IsNullOrEmpty(progress) ? "Starting Seed Generation" : progress;
        set => this.RaiseAndSetIfChanged(ref progress, value);
    }
    

    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> CancelGeneration { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> CopyError { get; }

    private CancellationTokenSource? tokenSource;

    private Exception? lastError;
    private bool hasError;
    [JsonIgnore]
    public bool HasError { get => hasError; set => this.RaiseAndSetIfChanged(ref hasError, value); }
    private bool isComplete;
    [JsonIgnore]
    public bool IsComplete { get => isComplete; set => this.RaiseAndSetIfChanged(ref isComplete, value); }

    [JsonIgnore]
    public MainViewModel Main { get; }
    // Reference to IScreen that owns the routable view model.
    [JsonIgnore]
    public IScreen HostScreen { get; }
    // Unique identifier for the routable view model.
    [JsonIgnore]
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    [JsonIgnore]
    public ViewModelActivator Activator { get; }
    
}