using js65;

namespace Desktop.Common;

public class DesktopJsEngine : ClearScriptEngine
{
    public DesktopJsEngine(Js65Options? options = null, bool debugJavascript = false)
        : base(options, true, debugJavascript)
    {
    }
}
