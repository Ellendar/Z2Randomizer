using System.Runtime.Versioning;
using js65;

namespace Desktop.Common;

[UnsupportedOSPlatform("browser")]
public class DesktopJsEngine(Js65Options? options = null, bool debugJavascript = false)
#pragma warning disable CA1416
    : ClearScriptEngine(options, null, debugJavascript)
{
    public override async Task<Js65CompileResult> Apply(byte[] rom, CancellationToken ct = default)
    {
        var dumpPath = Environment.GetEnvironmentVariable("JS65_DUMP_REQUEST");
        if (!string.IsNullOrEmpty(dumpPath))
        {
            File.WriteAllText(dumpPath, BuildRequestPublic());
        }
        return await base.Apply(rom, ct);
    }

    public string BuildRequestPublic() => BuildRequest();
}
#pragma warning restore CA1416
