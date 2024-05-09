using System.IO;
using System.Threading.Tasks;
using CrossPlatformUI.Services;

namespace CrossPlatformUI.Desktop;

public class DesktopFileService : IFileService
{
    public async Task<string> OpenLocalFile(string filename)
    {
        return await File.ReadAllTextAsync(filename);
    }
}