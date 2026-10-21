using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using ReactiveUI;
using RUISG = ReactiveUI.SourceGenerators;
using ReactiveUI.Primitives.Disposables;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using Z2Randomizer.RandomizerCore;
using Z2Randomizer.RandomizerCore.Sidescroll.Palace;
using CrossPlatformUI.Services;

namespace CrossPlatformUI.ViewModels;

[RequiresUnreferencedCode("")]
public partial class GenerateRomViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{

#pragma warning disable CS8618, CS9264

    [JsonConstructor]
    public GenerateRomViewModel() {}
#pragma warning restore CS8618, CS9264
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

        async void Randomize(MultipleDisposable disposables)
        {
            if (!Main.GenerateRomDialogOpen) return;

            runningMutex.Wait();
            IsRunning = true;

            lastError = null;
            HasError = false;
            IsComplete = false;
            tokenSource = new CancellationTokenSource();
            ProgressHeading = "Generating";
            ProgressBody = "Starting Seed Generation";
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
                        var version = Assembly.GetEntryAssembly()!.GetName().Version!;
                        var versionstr = $"{version.Major}.{version.Minor}.{version.Build}";
                        var filename = OutputFilenameFormatter.Format(
                            config.OutputFilenameTemplate,
                            flags,
                            config.Seed,
                            randomizer.Hash,
                            version: versionstr,
                            marioMode: config.MarioMode
                        );
                        var basename = Path.GetFileNameWithoutExtension(filename);
                        if (string.IsNullOrEmpty(basename))
                        {
                            basename = filename;
                        }
                        bool saved = false;
                        while (!saved)
                        {
                            try
                            {
                                await files.SaveGeneratedBinaryFile(filename, output.romdata!, Main.OutputFilePath);
                                saved = true;
                            }
                            catch (Exception e)
                            {
                                // DirectoryNotFoundException, UnauthorizedAccessException
                                // We could check for specific exceptions, but it's
                                // probably fine to do this for all errors while saving
                                var newFolder = await RandomizerViewModel.SelectSaveFolder();
                                if (!string.IsNullOrEmpty(newFolder) && newFolder != Main.OutputFilePath)
                                {
                                    Main.OutputFilePath = newFolder;
                                }
                                else // user did not pick a new save folder
                                {
                                    throw new UserFacingException("Unable to save ROM to folder", e.Message);
                                }
                            }
                        }
#if DEBUG
                        var debugfile = basename + ".mlb";
                        if (!string.IsNullOrEmpty(output.debuginfo))
                        {
                            await files.SaveSpoilerFile(debugfile, output.debuginfo, Main.OutputFilePath);
                        }
#endif
                        if (config.GenerateSpoiler)
                        {
                            var spoilerFilename = basename + "_spoiler.txt";
                            await files.SaveSpoilerFile(spoilerFilename, randomizer.GenerateSpoiler(), Main.OutputFilePath);
                            var spoilerMapFilename = basename + "_spoiler.png";
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
                    IsRunning = false;
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

    [JsonIgnore]
    [RUISG.Reactive]
    public partial string ProgressHeading { get; set; } = "";

    [JsonIgnore]
    [RUISG.Reactive]
    public partial string ProgressBody { get; set; } = "";

    [JsonIgnore]
    public ReactiveCommand<RxVoid, RxVoid> CancelGeneration { get; }
    [JsonIgnore]
    public ReactiveCommand<RxVoid, RxVoid> CopyError { get; }

    private readonly SemaphoreSlim runningMutex = new SemaphoreSlim(1, 1);
    [RUISG.Reactive]
    public partial bool IsRunning { get; set; }

    private CancellationTokenSource? tokenSource;

    private Exception? lastError;
    [JsonIgnore]
    [RUISG.Reactive]
    public partial bool HasError { get; set; }
    [JsonIgnore]
    [RUISG.Reactive]
    public partial bool IsComplete { get; set; }

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
