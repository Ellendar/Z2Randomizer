using System.Runtime.Versioning;
using js65;

namespace Desktop.Common;

[UnsupportedOSPlatform("browser")]
public class DesktopJsEngine(Js65Options? options = null, bool debugJavascript = false)
#pragma warning disable CA1416
    : ClearScriptEngine(options, true, debugJavascript);
#pragma warning restore CA1416
