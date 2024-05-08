using System.Threading.Tasks;

namespace CrossPlatformUI.Services;

public interface IFileService
{
    Task<string> OpenLocalFile(string filename);
}