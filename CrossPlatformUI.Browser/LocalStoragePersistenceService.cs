using System;
using System.IO;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace CrossPlatformUI.Browser;

public partial class LocalStoragePersistenceService : ISuspensionDriver
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
    
    public IObservable<object> LoadState()
    {
        return Task.Run(() =>
        {
            var data = GetItem("appstate");
            var ret = JsonConvert.DeserializeObject<object>(data ?? "{}", serializerSettings);
            return ret!;
        }).ToObservable();
    }

    public IObservable<Unit> SaveState(object state)
    {
        return Task.Run(Action).ToObservable();
        async void Action()
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
            await using var sw = new StringWriter(stringbuild);
            await using var writer = new JsonTextWriter(sw);
            writer.Formatting = Formatting.None;
            next.WriteTo(writer);
            SetItem("appstate", stringbuild.ToString());
        }
    }

    public IObservable<Unit> InvalidateState()
    {
        return Task.Run(Clear).ToObservable();
    }
}