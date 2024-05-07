using System;
using System.IO;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using CrossPlatformUI.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace CrossPlatformUI.Browser;

public partial class LocalStoragePersistenceService : ISuspendSyncService //: ISuspensionDriver
{
    [JSImport("globalThis.window.localStorage.setItem")]
    private static partial void SetItem(string key, string value);
    [JSImport("globalThis.window.localStorage.getItem")]
    private static partial string? GetItem(string key);
    [JSImport("globalThis.window.localStorage.clear")]
    private static partial void Clear();
    
    // internal static partial void Log([JSMarshalAs<JSType.String>] string message);
    // [JSImport("compile", "js65/js65.js")]
    // [return: JSMarshalAs<JSType.MemoryView>]
    // internal static partial Span<byte> Compile(string asm,
    //     [JSMarshalAs<JSType.MemoryView>] Span<byte> rom);
    
    private readonly JsonSerializerSettings serializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All,
    };
    
    public object LoadState()
    {
        var data = GetItem("appstate");
        var ret = JsonConvert.DeserializeObject<object>(data ?? "{}", serializerSettings);
        return ret!;
    }

    public void SaveState(object state)
    {
        var json = JsonConvert.SerializeObject(state, serializerSettings);
        var next = JObject.Parse(json);
        try
        {
            var settings = GetItem("appstate");
            var orig = JObject.Parse(settings ?? "{}");
            orig.Merge(next, new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union,
            });
            next = orig;
        }
        catch (Exception e) when (e is JsonException or IOException) { }

        var stringbuild = new StringBuilder();
        using var sw = new StringWriter(stringbuild);
        using var writer = new JsonTextWriter(sw);
        writer.Formatting = Formatting.None;
        next.WriteTo(writer);
        SetItem("appstate", stringbuild.ToString());
    }

    public void InvalidateState()
    {
        Clear();
    }
}