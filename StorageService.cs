using System;
using System.IO;
using System.Text.Json;
using DSOCompanion.Models;

namespace DSOCompanion.Services;

public sealed class StorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string AppFolder { get; }

    public string StateFile { get; }

    public StorageService()
    {
        AppFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DSO Companion");

        StateFile = Path.Combine(AppFolder, "state.json");

        Directory.CreateDirectory(AppFolder);
    }

    public AppState Load()
    {
        try
        {
            if (!File.Exists(StateFile))
            {
                return CreateDefaultState();
            }

            string json = File.ReadAllText(StateFile);
            AppState? state = JsonSerializer.Deserialize<AppState>(json, JsonOptions);

            return state ?? CreateDefaultState();
        }
        catch
        {
            return CreateDefaultState();
        }
    }

    public void Save(AppState state)
    {
        Directory.CreateDirectory(AppFolder);

        string json = JsonSerializer.Serialize(state, JsonOptions);
        File.WriteAllText(StateFile, json);
    }

    public void ExportBuild(BuildProfile build, string filePath)
    {
        if (build is null)
        {
            throw new ArgumentNullException(nameof(build));
        }

        string? folder = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(folder))
        {
            Directory.CreateDirectory(folder);
        }

        string json = JsonSerializer.Serialize(build, JsonOptions);
        File.WriteAllText(filePath, json);
    }

    public BuildProfile ImportBuild(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                "Die Build-Datei wurde nicht gefunden.",
                filePath);
        }

        string json = File.ReadAllText(filePath);
        BuildProfile? build = JsonSerializer.Deserialize<BuildProfile>(json, JsonOptions);

        if (build is null)
        {
            throw new InvalidDataException("Die Build-Datei ist ungültig.");
        }

        return build;
    }

    private static AppState CreateDefaultState()
    {
        CharacterProfile profile = DefaultDataService.CreateCharacter();

        return new AppState
        {
            Profiles = new List<CharacterProfile> { profile },
            ActiveProfileId = profile.Id,
            Mortis = DefaultDataService.CreateMortisPlan()
        };
    }
}
