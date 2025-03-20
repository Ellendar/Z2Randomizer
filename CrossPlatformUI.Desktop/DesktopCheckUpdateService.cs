using CrossPlatformUI.Services;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CrossPlatformUI.Desktop;

public class DesktopCheckUpdateService : ICheckUpdateService
{
    public Task CheckUpdate()
    {
        return Task.Run(() => WinSparkle.win_sparkle_check_update_with_ui()); 
    }
}
