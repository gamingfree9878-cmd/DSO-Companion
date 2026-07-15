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

    public static readonly string[] BuildTypes =
    [
        "Boss",
        "Farm",
        "PvE",
        "PvP",
        "Event",
        "Eigen"
    ];

    public static readonly string[] EquipmentSlots =
    [
        "Helm",
        "Schultern",
        "Torso",
        "Handschuhe",
        "Stiefel",
        "Waffe",
        "Nebenhand",
        "Amulett",
        "Ring 1",
        "Ring 2",
        "Gürtel",
        "Umhang",
        "Juwelen-Zier"
    ];

    public static readonly (string Name, string Color)[] GemTypes =
    [
        ("Rubin", "#D32F2F"),
        ("Onyx", "#20242B"),
        ("Rhodolith", "#EC407A"),
        ("Zirkon", "#FDD835"),
        ("Amethyst", "#8E44AD"),
        ("Eisdiamant", "#78909C"),
        ("Blitzdiamant", "#C0B84B"),
        ("Cyanit", "#26A69A"),
        ("Smaragd", "#0B6B3A"),
        ("Feuerdiamant", "#A94442"),
        ("Diamant", "#9E9E9E"),
        ("Andermachtdiamant", "#7E57C2"),
        ("Giftdiamant", "#558B2F")
    ];

    public static readonly (string Name, int Dust, int Gold)[] GemTiers =
    [
        ("Trapez Brillant", 5500, 5230),
        ("Trapez Exquisit", 8000, 7192),
        ("Imperial", 11000, 9480),
        ("Imperial Verfeinert", 14500, 12095),
        ("Imperial Brillant", 18500, 15038)
    ];

    public static CharacterProfile CreateCharacter(string name = "Mein Charakter")
    {
        BuildProfile build = CreateBuild("Boss-Build", "Boss");

        return new CharacterProfile
        {
            Name = name,
            CharacterClass = "Zirkelmagier",
            Level = 100,
            Server = "Heredur",
            Builds = [build],
            ActiveBuildId = build.Id,
            Gems = CreateGemCollections()
        };
    }

    public static BuildProfile CreateBuild(string name, string type = "Boss")
    {
        return new BuildProfile
        {
            Name = name,
            BuildType = type,
            Equipment = EquipmentSlots
                .Select(slot => new EquipmentSlot { SlotName = slot })
                .ToList()
        };
    }

    public static List<GemCollection> CreateGemCollections()
    {
        return GemTypes.Select(type => new GemCollection
        {
            GemName = type.Name,
            ColorHex = type.Color,
            Tiers = GemTiers.Select(tier => new GemTierEntry
            {
                TierName = tier.Name,
                DustCost = tier.Dust,
                GoldCost = tier.Gold,
                Quantity = 0
            }).ToList()
        }).ToList();
    }

    public static void NormalizeCharacter(CharacterProfile character)
    {
        character.Builds ??= [];
        character.Gems ??= [];

        if (character.Builds.Count == 0)
        {
            BuildProfile build = CreateBuild("Boss-Build", "Boss");
            character.Builds.Add(build);
            character.ActiveBuildId = build.Id;
        }

        foreach (BuildProfile build in character.Builds)
            NormalizeBuild(build);

        if (character.ActiveBuildId is null ||
            character.Builds.All(x => x.Id != character.ActiveBuildId))
        {
            character.ActiveBuildId = character.Builds[0].Id;
        }

        foreach ((string Name, string Color) gemType in GemTypes)
        {
            GemCollection? existing = character.Gems.FirstOrDefault(x => x.GemName == gemType.Name);

            if (existing is null)
            {
                existing = new GemCollection
                {
                    GemName = gemType.Name,
                    ColorHex = gemType.Color,
                    Tiers = []
                };
                character.Gems.Add(existing);
            }

            existing.ColorHex = gemType.Color;
            existing.Tiers ??= [];

            foreach ((string Name, int Dust, int Gold) tier in GemTiers)
            {
                GemTierEntry? tierEntry = existing.Tiers.FirstOrDefault(x => x.TierName == tier.Name);

                if (tierEntry is null)
                {
                    existing.Tiers.Add(new GemTierEntry
                    {
                        TierName = tier.Name,
                        DustCost = tier.Dust,
                        GoldCost = tier.Gold
                    });
                }
                else
                {
                    tierEntry.DustCost = tier.Dust;
                    tierEntry.GoldCost = tier.Gold;
                }
            }
        }
    }

    public static void NormalizeBuild(BuildProfile build)
    {
        build.Equipment ??= [];

        foreach (string slotName in EquipmentSlots)
        {
            if (build.Equipment.All(x => x.SlotName != slotName))
                build.Equipment.Add(new EquipmentSlot { SlotName = slotName });
        }
    }
}
