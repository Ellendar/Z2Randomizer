using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace CrossPlatformUI.Services;

public interface IFileDialogService
{
    public Task<IStorageFile?> OpenFileAsync();
    public Task<IStorageFolder?> OpenFolderAsync();
    public Task<IStorageFile?> SaveFileAsync();
}