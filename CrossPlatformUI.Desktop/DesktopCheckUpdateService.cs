using CrossPlatformUI.Services;
using System.Threading.Tasks;

namespace CrossPlatformUI.Desktop;

public class DesktopCheckUpdateService : ICheckUpdateService
{
    public Task CheckUpdate()
    {
        return Task.Run(() => WinSparkle.win_sparkle_check_update_with_ui()); 
    }
}
