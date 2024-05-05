using System.IO;
using System.Threading.Tasks;
using CrossPlatformUI.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrossPlatformUI.Desktop;

public class LocalStoragePersistenceService : IPersistenceService
{
    // TODO put this in appdata
    public const string SettingsFilename = "Settings.json";
    public async Task<string> Load()
    {
        var settings = await File.ReadAllTextAsync(SettingsFilename);
        return settings;
    }

    public async Task Update(string state)
    {
        var settings = await File.ReadAllTextAsync(SettingsFilename);
        var orig = JObject.Parse(settings);
        var next = JObject.Parse(state);
        orig.Merge(next, new JsonMergeSettings
        {
            // union array values together to avoid duplicates
            MergeArrayHandling = MergeArrayHandling.Union
        });

        await using var file = File.OpenWrite(SettingsFilename);
        await using var stream = new StreamWriter(file);
        await using var writer = new JsonTextWriter(stream);
        orig.WriteTo(writer);
    }
}