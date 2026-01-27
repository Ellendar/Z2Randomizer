using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CrossPlatformUI.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Validation.Helpers;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Sidescroll;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables.Fluent;

namespace CrossPlatformUI.ViewModels;

[RequiresUnreferencedCode("")]
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
            var flags = config.SerializeFlags();
            await clipboard.SetTextAsync($"""
Version: {versionstr}
Flags: {flags}
Seed: {config.Seed}
```
{lastError}
```
""");
            ProgressBody = "Error message copied to clipboard";
        });

        this.WhenActivated(Randomize);
        return;

        async void Randomize(CompositeDisposable disposables)
        {
            if (!Main.GenerateRomDialogOpen) return;

            runningMutex.Wait();
            isRunning.OnNext(true);

            lastError = null;
            HasError = false;
            IsComplete = false;
            tokenSource = new CancellationTokenSource();
            ProgressHeading = "";
            ProgressBody = "";
            await App.PersistState();
            var createAsm = App.Current?.Services?.GetService<Hyrule.NewAssemblerFn>();
            var files = App.Current?.Services?.GetService<IFileSystemService>();
            var host = (HostScreen as MainViewModel)!;
            var config = host.Config;
            var roomsJson = await files!.OpenFile(IFileSystemService.RandomizerPath.Palaces, "PalaceRooms.json");
            var customJson = config.UseCustomRooms ? await files.OpenFile(IFileSystemService.RandomizerPath.Palaces, "CustomRooms.json") : null;
            var rooms = config.UseCustomRooms ? customJson : roomsJson;
            var palaceRooms = new PalaceRooms(rooms!, config.UseCustomRooms);
            var randomizer = new Hyrule(createAsm!, palaceRooms);
            Dispatcher.UIThread.Post(GenerateSeed, DispatcherPriority.Background);
            return;
            
            async void GenerateSeed()
            {
                try
                {
                    var romdata = host.RomFileViewModel.RomData!.ToArray();
                    var output = await Task.Run(async () => await randomizer.Randomize(romdata, config, UpdateProgress, tokenSource.Token));
                    if(!tokenSource.IsCancellationRequested && output.success)
                    {
                        var flags = config.SerializeFlags();
                        var filename = $"Z2_{config.Seed}_{flags}.nes";
                        await files.SaveGeneratedBinaryFile(filename, output!, Main.OutputFilePath);
                        if (config.GenerateSpoiler)
                        {
                            var spoilerFilename = $"Z2_{config.Seed}_{flags}_spoiler.txt";
                            await files.SaveSpoilerFile(spoilerFilename, randomizer.GenerateSpoiler(), Main.OutputFilePath);
                            var spoilerMapFilename = $"Z2_{config.Seed}_{flags}_spoiler.png";
                            await files.SaveGeneratedBinaryFile(spoilerMapFilename, new Spoiler(randomizer.ROMData).CreateSpoilerImage(randomizer.worlds), Main.OutputFilePath);
                        }
                        ProgressHeading = "Generation Complete";
                        ProgressBody = $"Hash: {randomizer.Hash}\n\nFile: {filename}";
                    } else if (!output.success)
                    {
                        throw new Exception(output.messages);
                    }
                    IsComplete = true;
                }
                catch (Exception e)
                {
                    tokenSource.Cancel();
                    lastError = e;
                    HasError = true;
                    string errorHeading, errorBody;
                    if (e is UserFacingException userError)
                    {
                        errorHeading = userError.Heading;
                        errorBody = userError.Message;
                    }
                    else
                    {
#if DEBUG
                        // if (System.Diagnostics.Debugger.IsAttached) { throw; }
#endif
                        errorHeading = "Error Generating Seed";
                        errorBody = "Please report this on the discord";
                    }
                    await UpdateProgress(errorHeading, errorBody);
                }
                finally
                {
                    tokenSource.Dispose();
                    tokenSource = null;
                    isRunning.OnNext(false);
                    runningMutex.Release();
                }
            }
        }
    }

    private Task UpdateProgress(string body)
    {
        return Dispatcher.UIThread.InvokeAsync(() => { ProgressBody = body; }).GetTask();
    }

    private Task UpdateProgress(string heading, string body)
    {
        return Dispatcher.UIThread.InvokeAsync(() => { ProgressHeading = heading; ProgressBody = body; }).GetTask();
    }

    private string progressHeading = "";
    [JsonIgnore]
    public string ProgressHeading
    {
        get => string.IsNullOrEmpty(progressHeading) ? "Generating" : progressHeading;
        set => this.RaiseAndSetIfChanged(ref progressHeading, value);
    }

    private string progressBody = "";
    [JsonIgnore]
    public string ProgressBody {
        get => string.IsNullOrEmpty(progressBody) ? "Starting Seed Generation" : progressBody;
        set => this.RaiseAndSetIfChanged(ref progressBody, value);
    }

    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> CancelGeneration { get; }
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> CopyError { get; }

    private readonly SemaphoreSlim runningMutex = new SemaphoreSlim(1, 1);
    private readonly BehaviorSubject<bool> isRunning = new BehaviorSubject<bool>(false);
    public IObservable<bool> IsRunning => isRunning;

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