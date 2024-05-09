using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using CrossPlatformUI.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace CrossPlatformUI.Desktop;

public class LocalFilePersistenceService : ISuspendSyncService // : ISuspensionDriver
{
    // TODO put this in appdata
    public const string SettingsFilename = "Settings.json";
    
    private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };
    
    public object? LoadState()
    {
        var data = File.ReadAllText(SettingsFilename);
        return JsonConvert.DeserializeObject<object>(data, serializerSettings);
    }

    public void SaveState(object state)
    {
        var json = JsonConvert.SerializeObject(state, serializerSettings);
        var next = JObject.Parse(json);
        try
        {
            var settings = File.ReadAllText(SettingsFilename);
            var orig = JObject.Parse(settings);
            orig.Merge(next, new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union,
            });
            next = orig;
        }
        catch (Exception e) when (e is JsonException or IOException) { }

        using var file = File.OpenWrite(SettingsFilename);
        using var stream = new StreamWriter(file);
        using var writer = new JsonTextWriter(stream);
        next.WriteTo(writer);
    }

    public void InvalidateState()
    {
        try
        {
            File.Delete(SettingsFilename);
        } catch (IOException e) {}
    }
}