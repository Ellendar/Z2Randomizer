using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Z2Randomizer.UI;


class WinSparkle
{
    public const string DLL_NAME = "WinSparkle_0_8_0_x64.dll";
    // Note that some of these functions are not implemented by WinSparkle YET.
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_init();
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_cleanup();
    [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_set_appcast_url(String url);
    [DllImport(DLL_NAME, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_set_app_details(String company_name,
                                         String app_name,
                                         String app_version);
    [DllImport(DLL_NAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_set_registry_path(String path);
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
    public static extern void win_sparkle_check_update_with_ui();
}
