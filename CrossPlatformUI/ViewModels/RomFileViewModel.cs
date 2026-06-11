using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using SD.Tools.BCLExtensions.CollectionsRelated;
using Z2Randomizer.RandomizerCore;
using CrossPlatformUI.Services;

namespace CrossPlatformUI.ViewModels;

[RequiresUnreferencedCode("ReactiveUI uses reflection")]
public class RomFileViewModel : ViewModelBase, IRoutableViewModel
{
    private byte[] romData = [];
    public byte[] RomData
    {
        get => romData;
        set
        {
            this.RaiseAndSetIfChanged(ref romData, value);
            this.RaisePropertyChanged(nameof(HasRomData));
        }
    }

    private string message { get; set; } = "Select your Zelda 2 ROM to get started!";
    [JsonIgnore]
    public string Message
    {
        get => message;
        set { message = value; this.RaisePropertyChanged(); }
    }

    [JsonIgnore]
    public IObservable<byte[]> RomDataObservable => this.WhenAnyValue(x => x.RomData);

    [JsonIgnore]
    public bool HasRomData => !RomData.IsNullOrEmpty();

    [JsonIgnore]
    public IObservable<bool> HasRomDataObservable => this.WhenAnyValue(x => x.HasRomData);

    [JsonConstructor]
#pragma warning disable CS8618
    public RomFileViewModel() {}
#pragma warning restore CS8618

    public RomFileViewModel(MainViewModel main)
    {
        Main = main;
        OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileInternal);
        HostScreen = Main;
    }
    

    private async Task OpenFileInternal(CancellationToken token)
    {
        var filesService = App.Current?.Services?.GetService<IFileDialogService>();
        if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

        var file = await filesService.OpenFileAsync();
        if (file is null) return;

        var fileprops = await file.GetBasicPropertiesAsync();
        if (fileprops.Size <= 1024 * 1024 * 1)
        {
            await using var readStream = await file.OpenReadAsync();
            byte[] fileData = new byte[(uint)fileprops.Size];
            var read = await readStream.ReadAsync(fileData, token);
            try
            {
                ROM.ValidateVanillaRom(ref fileData);
            }
            catch (UserFacingException e)
            {
                Message = e.Message;
                return;
            }
            RomData = fileData;
            if (OperatingSystem.IsBrowser())
            {
                // Manually save the state 
                await App.PersistState();
            }
            else
            {
                // This part crashes if run in the browser build
                if ((Main.OutputFilePath ?? "") == "")
                {
                    Main.OutputFilePath = new Uri(file.Path, ".").LocalPath;
                }
            }
            HostScreen.Router.NavigateBack.Execute();
        }
        else
        {
            Message = "File exceeded 1MB limit. Please provide an unmodified Zelda 2 ROM (US release) with or without header.";
        }
    }
    
    [JsonIgnore]
    public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }
    [JsonIgnore]
    public MainViewModel Main { get; }
    [JsonIgnore]
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    [JsonIgnore]
    public IScreen HostScreen { get; }

}