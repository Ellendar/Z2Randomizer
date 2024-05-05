using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace CrossPlatformUI.Services;

public interface IFileDialogService
{
    public Task<IStorageFile?> OpenFileAsync();
    public Task<IStorageFile?> SaveFileAsync();
}