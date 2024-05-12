using System.Collections.Generic;
using System.Threading.Tasks;

namespace CrossPlatformUI.Services;

public interface IFileService
{
    Task<string> OpenLocalFile(string filename);
    Task<byte[]> OpenLocalBinaryFile(string filename);
    Task<IEnumerable<string>> ListLocalFiles(string path);
    Task SaveGeneratedBinaryFile(string filename, byte[] filedata, string? path = null);
}