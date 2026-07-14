using DSOCompanion.Models;

namespace DSOCompanion.Services;

public static class AppDataService
{
    public static readonly string[] Classes =
    [
        "Zirkelmagier",
        "Drachenkrieger",
        "Waldläufer",
        "Dampfmechanikus"
    ];

    public static readonly string[] Servers =
    [
        "Heredur",
        "Grimmag",
        "Werian",
        "Tegan",
        "Balor"
    ];

    public static CharacterProfile CreateCharacter(string name = "Mein Charakter")
    {
        var build = new BuildProfile { Name = "Boss-Build" };

        return new CharacterProfile
        {
            Name = name,
            CharacterClass = "Zirkelmagier",
            Level = 100,
            Server = "Heredur",
            Builds = [build],
            ActiveBuildId = build.Id
        };
    }
}
