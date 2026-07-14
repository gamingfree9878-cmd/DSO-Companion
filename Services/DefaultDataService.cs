using DSOCompanion.Models;

namespace DSOCompanion.Services;

public static class DefaultDataService
{
    public static readonly string[] Slots =
    [
        "Helm", "Schultern", "Torso", "Handschuhe", "Stiefel", "Waffe",
        "Nebenhand", "Amulett", "Ring 1", "Ring 2", "Gürtel", "Umhang", "Juwelen-Zier"
    ];

    public static CharacterProfile CreateCharacter(string name = "Mein Charakter")
    {
        var build = CreateBuild("Boss-Build");
        return new CharacterProfile
        {
            Name = name,
            Builds = [build],
            ActiveBuildId = build.Id
        };
    }

    public static BuildProfile CreateBuild(string name)
    {
        return new BuildProfile
        {
            Name = name,
            Equipment = Slots.Select(x => new EquipmentSlot { SlotName = x }).ToList()
        };
    }

    public static MortisPlan CreateMortisPlan()
    {
        return new MortisPlan
        {
            Activities =
            [
                new() { Name = "Heredur", Difficulty = "-", BonesPerRun = 5 },
                new() { Name = "Sargon", Difficulty = "-", BonesPerRun = 5 },
                new() { Name = "Herold", Difficulty = "-", BonesPerRun = 5 },
                new() { Name = "Nefertari", Difficulty = "-", BonesPerRun = 5 },
                new() { Name = "Balor", Difficulty = "-", BonesPerRun = 5 },

                new() { Name = "Mortis", Difficulty = "Infernal", BonesPerRun = 25 },
                new() { Name = "Mortis", Difficulty = "Gnadenlos", BonesPerRun = 35 },
                new() { Name = "Mortis", Difficulty = "Blutvergießen", BonesPerRun = 35 },

                new() { Name = "Inferno", Difficulty = "Infernal", BonesPerRun = 40 },
                new() { Name = "Inferno", Difficulty = "Gnadenlos", BonesPerRun = 50 },
                new() { Name = "Inferno", Difficulty = "Blutvergießen", BonesPerRun = 60 },

                new() { Name = "Wächter", Difficulty = "-", BonesPerRun = 5 },
                new() { Name = "Gruppe Wächter", Difficulty = "-", BonesPerRun = 25 }
            ]
        };
    }
}
