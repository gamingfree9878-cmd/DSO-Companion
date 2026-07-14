using System.Text.Json;
using DSOCompanion.Models;

namespace DSOCompanion.Services;

public sealed class StorageService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    private readonly string _folder;
    private readonly string _file;

    public StorageService()
    {
        _folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DSO Companion");

        _file = Path.Combine(_folder, "state.json");

        Directory.CreateDirectory(_folder);
    }

    public AppState Load()
    {
        try
        {
            if (!File.Exists(_file))
                return CreateDefault();

            string json = File.ReadAllText(_file);
            return JsonSerializer.Deserialize<AppState>(json, Options) ?? CreateDefault();
        }
        catch
        {
            return CreateDefault();
        }
    }

    public void Save(AppState state)
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(_file, JsonSerializer.Serialize(state, Options));
    }

    private static AppState CreateDefault()
    {
        CharacterProfile character = AppDataService.CreateCharacter();

        return new AppState
        {
            Characters = [character],
            ActiveCharacterId = character.Id
        };
    }
}
