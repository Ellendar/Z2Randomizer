using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReactiveUI;

namespace CrossPlatformUI.Desktop;

public class LocalFilePersistenceService : ISuspensionDriver
{
    // TODO put this in appdata
    public const string SettingsFilename = "Settings.json";
    
    private readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All
    };
    
    public IObservable<object> LoadState()
    {
        return Task.Run(async () =>
        {
            var data = await File.ReadAllTextAsync(SettingsFilename);
            return JsonConvert.DeserializeObject<object>(data, serializerSettings)!;
        }).ToObservable();
    }

    public IObservable<Unit> SaveState(object state)
    {
        return Task.Run(async () =>
        {
            var json = JsonConvert.SerializeObject(state, serializerSettings);
            var next = JObject.Parse(json);
            try
            {
                var settings = await File.ReadAllTextAsync(SettingsFilename);
                var orig = JObject.Parse(settings);
                orig.Merge(next, new JsonMergeSettings
                {
                    // union array values together to avoid duplicates
                    MergeArrayHandling = MergeArrayHandling.Union,
                });
                next = orig;
            }
            catch (Exception e) when (e is JsonException or IOException) { }

            await using var file = File.OpenWrite(SettingsFilename);
            await using var stream = new StreamWriter(file);
            await using var writer = new JsonTextWriter(stream);
            next.WriteTo(writer);
        }).ToObservable();
    }

    public IObservable<Unit> InvalidateState()
    {
        return Task.Run(() =>
        {
            try
            {
                File.Delete(SettingsFilename);
            } catch (IOException e) {}
            return Task.CompletedTask;
        }).ToObservable();
    }
}