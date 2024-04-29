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
    public string? UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    public IScreen HostScreen { get; }

    public byte[]? RomData { get; private set; }

    public RomFileViewModel(IScreen hostScreen)
    {
        OpenFileCommand = ReactiveCommand.CreateFromTask(OpenFileInternal);
        HostScreen = hostScreen;
    }
    
    public ReactiveCommand<Unit, Unit> OpenFileCommand { get; }
    async Task OpenFileInternal(CancellationToken token)
    {
        try
        {
            var filesService = App.Current?.Services?.GetService<IFilesService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.OpenFileAsync();
            if (file is null) return;

            if ((await file.GetBasicPropertiesAsync()).Size <= 1024 * 1024 * 1)
            {
                await using var readStream = await file.OpenReadAsync();
                using var reader = new BinaryReader(readStream);
                RomData = ReadAllBytes(reader);
            }
            else
            {
                throw new Exception("File exceeded 1MB limit.");
            }
        }
        catch (Exception e)
        {
            // ErrorMessages?.Add(e.Message);
        }

        HostScreen.Router.NavigateBack.Execute();
    }

    private static byte[] ReadAllBytes(BinaryReader reader)
    {
        const int bufferSize = 4096;
        using var ms = new MemoryStream();
        var buffer = new byte[bufferSize];
        int count;
        while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
            ms.Write(buffer, 0, count);
        return ms.ToArray();
    }
}