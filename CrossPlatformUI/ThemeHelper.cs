using Avalonia.Media;
using Material.Styles.Themes;

namespace CrossPlatformUI;

/// Material Avalonia's dynamic theming system leaves a lot to be desired.
/// Probably base Avalonia is better for this & better documented.
public class ThemeHelper
{
    private static readonly Theme DarkMaterialTheme;
    private static readonly Theme LightMaterialTheme;

    private static IBrush DarkNonStandardFlagEnabledBackground = SolidColorBrush.Parse("#80b10600");
    private static IBrush LightNonStandardFlagEnabledBackground = SolidColorBrush.Parse("#d2b30000");

    static ThemeHelper() {
        DarkMaterialTheme = Theme.Create(Theme.Dark, Color.Parse("#ffbc33"), Color.Parse("#969696"));
        DarkMaterialTheme.Paper = Color.Parse("#1a1a1a");

        LightMaterialTheme = Theme.Create(Theme.Light, Color.Parse("#002a88"), Color.Parse("#696969"));
    }

    public static string SetTheme(string name)
    {
        var app = App.Current;
        if (app == null)
        {
            return "Light";
        }
        var themeBootstrap = app.LocateMaterialTheme<MaterialTheme>();
        switch (name)
        {
            case "Dark":
                themeBootstrap.CurrentTheme = DarkMaterialTheme;
                return "Dark";
            case "Light":
            default:
                themeBootstrap.CurrentTheme = LightMaterialTheme;
                return "Light";
        }
    }

    public static IBrush GetFlagAlertBackgroundBrush(string theme, bool alert)
    {
        if (alert)
        {
            return theme == "Dark" ? DarkNonStandardFlagEnabledBackground : LightNonStandardFlagEnabledBackground;
        }
        else
        {
            return Brushes.Transparent;
        }
    }
}
