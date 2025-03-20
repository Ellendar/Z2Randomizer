using System.Runtime.InteropServices;

namespace CrossPlatformUI.Desktop;

class WinSparkle
{
    public const string DLL_NAME = "WinSparkle_0_8_0_x64.dll";
    // Note that some of these functions are not implemented by WinSparkle YET.
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_init();
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_cleanup();
    [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_set_appcast_url(string url);
    [DllImport(DLL_NAME, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_set_app_details(string company_name,
                                         string app_name,
                                         string app_version);
    [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_set_registry_path(string path);
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_check_update_with_ui();
}
