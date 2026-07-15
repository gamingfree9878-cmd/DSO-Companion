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

    public static readonly Dictionary<string, string> GemImages = new()
    {
        ["Rubin"] = "/Assets/Gems/ruby.png",
        ["Onyx"] = "/Assets/Gems/onyx.png",
        ["Rhodolith"] = "/Assets/Gems/rhodolith.png",
        ["Zirkon"] = "/Assets/Gems/zircon.png",
        ["Amethyst"] = "/Assets/Gems/amethyst.png",
        ["Cyanit"] = "/Assets/Gems/cyanite.png",
        ["Smaragd"] = "/Assets/Gems/emerald.png",
        ["Diamant"] = "/Assets/Gems/diamond.png",
        ["Eisdiamant"] = "/Assets/Gems/ice.png",
        ["Blitzdiamant"] = "/Assets/Gems/lightning.png",
        ["Feuerdiamant"] = "/Assets/Gems/fire.png",
        ["Andermachtdiamant"] = "/Assets/Gems/andermacht.png",
        ["Giftdiamant"] = "/Assets/Gems/poison.png",
    };

    public static readonly (string Name, string ColorName, string Color, string Category)[] GemTypes =
    [
        ("Rubin", "Rot", "#FF3B30", "Offensiv"),
        ("Onyx", "Schwarz", "#59616D", "Offensiv"),
        ("Rhodolith", "Pink", "#FF2D96", "Offensiv"),
        ("Zirkon", "Gelb", "#D7C800", "Offensiv"),
        ("Amethyst", "Lila", "#9C4DFF", "Defensiv"),
        ("Cyanit", "Türkis", "#20C6C7", "Defensiv"),
        ("Smaragd", "Dunkelgrün", "#149447", "Defensiv"),
        ("Eisdiamant", "Grau / Blau", "#70A8C5", "Diamanten"),
        ("Blitzdiamant", "Grau / Gelb", "#D0B84F", "Diamanten"),
        ("Feuerdiamant", "Grau / Rot", "#D04A45", "Diamanten"),
        ("Diamant", "Grau", "#AEB7C2", "Diamanten"),
        ("Andermachtdiamant", "Grau / Lila", "#8A64C8", "Diamanten"),
        ("Giftdiamant", "Grau / Grün", "#6F9D4B", "Diamanten")
    ];

    public static readonly (string Name, int Dust, int Gold)[] GemTiers =
    [
        ("Trapez", 2000, 2288),
        ("Trapez Verfeinert", 3500, 3596),
        ("Trapez Brillant", 5500, 5230),
        ("Trapez Exquisit", 8000, 7192),
        ("Imperial", 11000, 9480),
        ("Imperial Verfeinert", 14500, 12095),
        ("Imperial Brillant", 18500, 15038),
        ("Imperial Exquisit", 23000, 18307)
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
            ColorName = type.ColorName,
            ColorHex = type.Color,
            Category = type.Category,
            ImagePath = GemImages.TryGetValue(type.Name, out string? imagePath)
                ? imagePath
                : "",
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

        foreach ((string Name, string ColorName, string Color, string Category) gemType in GemTypes)
        {
            GemCollection? existing = character.Gems.FirstOrDefault(x => x.GemName == gemType.Name);

            if (existing is null)
            {
                existing = new GemCollection
                {
                    GemName = gemType.Name,
                    Tiers = []
                };
                character.Gems.Add(existing);
            }

            existing.ColorName = gemType.ColorName;
            existing.ColorHex = gemType.Color;
            existing.Category = gemType.Category;
            existing.ImagePath = GemImages.TryGetValue(gemType.Name, out string? imagePath)
                ? imagePath
                : "";
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
