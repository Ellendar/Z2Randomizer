using System;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using CrossPlatformUI.Services;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;

namespace CrossPlatformUI.ViewModels;

public class RomFileViewModel : ViewModelBase, IRoutableViewModel
{
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString()[..5];
    public IScreen HostScreen { get; }

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
        var filesService = App.Current?.Services?.GetService<IFilesService>();
        if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

        var file = await filesService.OpenFileAsync();
        if (file is null) return;

        var fileprops = await file.GetBasicPropertiesAsync();
        if (fileprops.Size <= 1024 * 1024 * 1)
        {
            await using var readStream = await file.OpenReadAsync();
            RomData = new byte[(uint)fileprops.Size];
            var read = await readStream.ReadAsync(RomData, token);
            if (read == 1024 * 256 + 0x10)
            {
                // I dunno
                HasRomData = true;
                HostScreen.Router.NavigateBack.Execute();
            }
        }
        else
        {
            throw new Exception("File exceeded 1MB limit.");
        }
    }
}