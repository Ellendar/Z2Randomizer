using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CrossPlatformUI.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrossPlatformUI.Desktop;

[RequiresUnreferencedCode("Newtonsoft.Json uses reflection")]
public class LocalFilePersistenceService : ISuspendSyncService // : ISuspensionDriver
{
    public string? SettingsPath;

    public LocalFilePersistenceService()
    {
        if (OperatingSystem.IsWindows())
        {
            
        }
        else if (OperatingSystem.IsMacOS())
        {
            
        }
    }
    
    private readonly JsonSerializerSettings serializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.All
    };
    
    public object? LoadState()
    {
        var data = File.ReadAllText(App.SETTINGS_FILENAME);
        return JsonConvert.DeserializeObject<object>(data, serializerSettings);
    }

    public void SaveState(object state)
    {
        var json = JsonConvert.SerializeObject(state, serializerSettings);
        var next = JObject.Parse(json);
        try
        {
            var settings = File.ReadAllText(App.SETTINGS_FILENAME);
            var orig = JObject.Parse(settings);
            orig.Merge(next, new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union,
            });
            next = orig;
        }
        catch (Exception e) when (e is JsonException or IOException) { }

        using var file = File.CreateText(App.SETTINGS_FILENAME);
        using var writer = new JsonTextWriter(file);
        next.WriteTo(writer);
    }

    public void InvalidateState()
    {
        try
        {
            File.Delete(App.SETTINGS_FILENAME);
        } catch (IOException) {}
    }
    
    public Task<IEnumerable<string>> ListLocalFiles(string path)
    {
        return Task.FromResult(Directory.GetFiles(path).AsEnumerable());
    }
}
