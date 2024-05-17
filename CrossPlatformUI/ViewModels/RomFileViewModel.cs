using System;
using System.IO;
using System.Reactive;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CrossPlatformUI.Services;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using SD.Tools.BCLExtensions.CollectionsRelated;

namespace CrossPlatformUI.ViewModels;

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

    [JsonIgnore]
    public bool HasRomData => !RomData.IsNullOrEmpty();

    [JsonConstructor]
    public RomFileViewModel() {}

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
            var tmp = new byte[(uint)fileprops.Size];
            var read = await readStream.ReadAsync(tmp, token);
            // TODO: Better validation
            if (read == 1024 * 256 + 0x10)
            {
                RomData = tmp;
                HostScreen.Router.NavigateBack.Execute();
                // Manually save the state 
                if (OperatingSystem.IsBrowser())
                {
                    await App.PersistState();
                }
            }
        }
        else
        {
            throw new Exception("File exceeded 1MB limit.");
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