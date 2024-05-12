using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossPlatformUI.Services;

namespace CrossPlatformUI.Desktop;

public class DesktopFileService : IFileService
{
    public async Task<string> OpenLocalFile(string filename)
    {
        return await File.ReadAllTextAsync(filename);
    }
    public async Task<byte[]> OpenLocalBinaryFile(string filename)
    {
        return await File.ReadAllBytesAsync(filename);
    }

    public Task SaveGeneratedBinaryFile(string filename, byte[] filedata, string? path = null)
    {
        var file = Path.Join(path ?? "", filename);
        return File.WriteAllBytesAsync(file, filedata);
    }
}