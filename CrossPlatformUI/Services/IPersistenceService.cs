using System.Threading.Tasks;

namespace CrossPlatformUI.Services;

public interface IPersistenceService
{
    public Task<string> Load();
    public Task Update(string state);
}
