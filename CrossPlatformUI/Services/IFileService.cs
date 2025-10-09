using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossPlatformUI.Services;

public interface IFileSystemService
{
    public enum RandomizerPath
    {
        Sprites,
        Settings,
        Palaces,
    }
    Task<string> OpenFile(RandomizerPath path, string filename);
    string OpenFileSync(RandomizerPath path, string filename);
    Task<byte[]> OpenBinaryFile(RandomizerPath path, string filename);
    Task SaveFile(RandomizerPath path, string filename, string data);
    Task<IEnumerable<string>> ListLocalFiles(RandomizerPath path);
    Task SaveGeneratedBinaryFile(string filename, byte[] filedata, string? path = null);
    Task SaveSpoilerFile(string filename, string data, string? path = null);
}