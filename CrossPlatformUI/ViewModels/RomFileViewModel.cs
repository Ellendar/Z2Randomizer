using System;
using System.IO;
using System.Reactive;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CrossPlatformUI.Services;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;

namespace CrossPlatformUI.ViewModels;

[DataContract]
public class RomFileViewModel : ViewModelBase, IRoutableViewModel
{
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }

    [DataMember]
    public byte[]? RomData { get; private set; }

    public bool HasRomData { get => RomData != null; set => this.RaisePropertyChanged(); }

    public RomFileViewModel(IScreen hostScreen)
    {
        OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileInternal);
        HostScreen = hostScreen;
    }
    
    public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }

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
            RomData = new byte[(uint)fileprops.Size];
            var read = await readStream.ReadAsync(RomData, token);
            // TODO: Better validation
            if (read == 1024 * 256 + 0x10)
            {
                HasRomData = true;
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
}