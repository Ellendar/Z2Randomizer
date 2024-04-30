using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace CrossPlatformUI.Services;

public interface IFilesService
{
    public Task<IStorageFile?> OpenFileAsync();
    public Task<IStorageFile?> SaveFileAsync();
}