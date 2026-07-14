using System.Text.Json;
using DSOCompanion.Models;

namespace DSOCompanion.Services;

public sealed class StorageService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public string AppFolder { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DSO Companion");

    public string StateFile => Path.Combine(AppFolder, "state.json");

    public StorageService()
    {
        Directory.CreateDirectory(AppFolder);
    }

    public AppState Load()
    {
        try
        {
            if (!File.Exists(StateFile))
                return CreateDefault();

            var json = File.ReadAllText(StateFile);
            var state = JsonSerializer.Deserialize<AppState>(json, Options);
            return state ?? CreateDefault();
        }
        catch
        {
            return CreateDefault();
        }
    }

    public void Save(AppState state)
    {
        Directory.CreateDirectory(AppFolder);
        File.WriteAllText(StateFile, JsonSerializer.Serialize(state, Options));
    }

    public void ExportBuild(BuildProfile build, string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(build, Options));
    }

    public BuildProfile ImportBuild(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<BuildProfile>(json, Options)
               ?? throw new InvalidDataException("Ungültige Build-Datei.");
    }

    private static AppState CreateDefault()
    {
        var profile = DefaultDataService.CreateCharacter();
        return new AppState
        {
            Profiles = [profile],
            ActiveProfileId = profile.Id,
            Mortis = DefaultDataService.CreateMortisPlan()
        };
    }
}
