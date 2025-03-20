using CrossPlatformUI.Services;
using System.Threading.Tasks;

namespace CrossPlatformUI.Browser;

public class BrowserCheckUpdateService : ICheckUpdateService
{
    public Task CheckUpdate()
    {
        //The browser implementation is inherently up to date, so do nothing.
        return Task.CompletedTask;
    }
}
