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
        "Helm", "Schultern", "Torso", "Handschuhe", "Stiefel", "Waffe",
        "Nebenhand", "Amulett", "Ring 1", "Ring 2", "Gürtel", "Umhang", "Juwelen-Zier"
    ];

    public static readonly (string Name, string ColorName, string Color, string Category)[] GemTypes =
    [
        ("Rubin", "Rot", "#FF3B30", "Offensiv"),
        ("Onyx", "Schwarz", "#59616D", "Offensiv"),
        ("Rhodolith", "Pink", "#FF2D96", "Offensiv"),
        ("Zirkon", "Gelb", "#D7C800", "Offensiv"),
        ("Amethyst", "Lila", "#9C4DFF", "Defensiv"),
        ("Cyanit", "Türkis", "#20C6C7", "Defensiv"),
        ("Smaragd", "Dunkelgrün", "#149447", "Defensiv"),
        ("Diamant", "Grau", "#AEB7C2", "Diamanten"),
        ("Eisdiamant", "Grau / Blau", "#70A8C5", "Diamanten"),
        ("Blitzdiamant", "Grau / Gelb", "#D0B84F", "Diamanten"),
        ("Feuerdiamant", "Grau / Rot", "#D04A45", "Diamanten"),
        ("Andermachtdiamant", "Grau / Lila", "#8A64C8", "Diamanten"),
        ("Giftdiamant", "Grau / Grün", "#6F9D4B", "Diamanten"),
        ("Opal", "Regenbogen", "#63D7D1", "Opal")
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
        ["Opal"] = "/Assets/Gems/opal.png"
    };

    // Exakte Werte aus Steine_Rechnerd.xlsx
    // Offensiv: Rubin, Onyx, Zirkon, Rhodolith
    public static readonly (string Name, int Dust, int Gold)[] OffensiveGemTiers =
    [
        ("Trapez", 2000, 2288),
        ("Trapez Verfeinerter", 3500, 3596),
        ("Trapez Brillanter", 5500, 5230),
        ("Trapez Exquisit", 8000, 7192),
        ("Imperial", 11000, 9480),
        ("Imperial Verfeinerter", 14500, 12095),
        ("Imperial Brillanter", 18500, 15038),
        ("Imperial Exquisit", 23000, 18307),
        ("Maximal", 0, 0)
    ];

    // Defensiv: Amethyst, Cyanit, Smaragd und alle Diamanten
    public static readonly (string Name, int Dust, int Gold)[] DefensiveGemTiers =
    [
        ("Trapez", 1600, 1830),
        ("Trapez Verfeinerter", 2800, 2876),
        ("Trapez Brillanter", 4400, 4184),
        ("Trapez Exquisit", 6400, 5753),
        ("Imperial", 8800, 7584),
        ("Imperial Verfeinerter", 11600, 9676),
        ("Imperial Brillanter", 14800, 12030),
        ("Imperial Exquisit", 18400, 14645),
        ("Maximal", 0, 0)
    ];

    // Opale besitzen eine eigene Kostenreihe.
    public static readonly (string Name, int Dust, int Gold)[] OpalTiers =
    [
        ("Trapez", 4500, 0),
        ("Trapez Verfeinerter", 7875, 20138),
        ("Trapez Brillanter", 12375, 29291),
        ("Trapez Exquisit", 18000, 40276),
        ("Imperial", 25700, 53091),
        ("Imperial Verfeinerter", 32625, 67737),
        ("Imperial Brillanter", 41625, 84214),
        ("Imperial Exquisit", 51750, 102521),
        ("Maximal", 0, 0)
    ];

    private static (string Name, int Dust, int Gold)[] GetGemTiers(string category) =>
        category switch
        {
            "Defensiv" => DefensiveGemTiers,
            "Diamanten" => DefensiveGemTiers,
            "Opal" => OpalTiers,
            _ => OffensiveGemTiers
        };

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
            Gems = CreateGemCollections(),
            Runes = CreateRuneCollections(),
            Mortis = CreateMortisPlan(),
            Varnok = CreateVarnokPlan()
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
        List<GemCollection> collections = GemTypes.Select(type => new GemCollection
        {
            GemName = type.Name,
            ColorName = type.ColorName,
            ColorHex = type.Color,
            Category = type.Category,
            ImagePath = GemImages[type.Name],
            Tiers = GetGemTiers(type.Category).Select(tier => new GemTierEntry
            {
                TierName = tier.Name,
                DustCost = tier.Dust,
                GoldCost = tier.Gold
            }).ToList()
        }).ToList();

        foreach (GemCollection collection in collections)
            collection.AttachTierNotifications();

        return collections;
    }

    public static List<RuneTierEntry> CreateRuneTiers(string category)
    {
        (int Dust, int Gold)[] values = category switch
        {
            "Offensiv" =>
            [
                (3126, 8631),
                (8596, 17261),
                (17188, 29031),
                (28908, 43937)
            ],
            "Defensiv" =>
            [
                (2500, 4315),
                (6876, 8630),
                (13750, 14515),
                (23126, 21968)
            ],
            _ =>
            [
                (250, 2877),
                (688, 5753),
                (1375, 9677),
                (7492, 14645)
            ]
        };

        string[] names = ["Grün", "Blau", "Lila", "Legendär"];

        return names.Select((name, index) => new RuneTierEntry
        {
            TierName = name,
            DustCost = values[index].Dust,
            GoldCost = values[index].Gold
        }).ToList();
    }

    public static List<RuneCollection> CreateRuneCollections()
    {
        List<RuneCollection> collections =
        [
        new RuneCollection
        {
            RuneName = "Konzentrierte Herbstrune",
            Category = "Offensiv",
            ImagePath = "/Assets/Runes/concentrated_autumn.png",
            Tiers = CreateRuneTiers("Offensiv")
        },
        new RuneCollection
        {
            RuneName = "Konzentrierte Sommerrune",
            Category = "Offensiv",
            ImagePath = "/Assets/Runes/concentrated_summer.png",
            Tiers = CreateRuneTiers("Offensiv")
        },
        new RuneCollection
        {
            RuneName = "Konzentrierte Frühlingsrune",
            Category = "Offensiv",
            ImagePath = "/Assets/Runes/concentrated_spring.png",
            Tiers = CreateRuneTiers("Offensiv")
        },
        new RuneCollection
        {
            RuneName = "Konzentrierte Mittwinterrune",
            Category = "Offensiv",
            ImagePath = "/Assets/Runes/concentrated_midwinter.png",
            Tiers = CreateRuneTiers("Offensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune der Beschleunigung",
            Category = "Offensiv",
            ImagePath = "/Assets/Runes/acceleration.png",
            Tiers = CreateRuneTiers("Offensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune der Macht",
            Category = "Offensiv",
            ImagePath = "/Assets/Runes/power.png",
            Tiers = CreateRuneTiers("Offensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune der Vitalität",
            Category = "Defensiv",
            ImagePath = "/Assets/Runes/vitality.png",
            Tiers = CreateRuneTiers("Defensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune der Tapferkeit",
            Category = "Defensiv",
            ImagePath = "/Assets/Runes/bravery.png",
            Tiers = CreateRuneTiers("Defensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune der Schnelligkeit",
            Category = "Offensiv",
            ImagePath = "/Assets/Runes/speed.png",
            Tiers = CreateRuneTiers("Offensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune der Verwüstung",
            Category = "Offensiv",
            ImagePath = "/Assets/Runes/devastation.png",
            Tiers = CreateRuneTiers("Offensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune des Andermantfiebers",
            Category = "Defensiv",
            ImagePath = "/Assets/Runes/andermant_fever.png",
            Tiers = CreateRuneTiers("Defensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune der Ausdauer",
            Category = "Defensiv",
            ImagePath = "/Assets/Runes/endurance.png",
            Tiers = CreateRuneTiers("Defensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune der Aufladung",
            Category = "Offensiv",
            ImagePath = "/Assets/Runes/charge.png",
            Tiers = CreateRuneTiers("Offensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune der Effizienz",
            Category = "Offensiv",
            ImagePath = "/Assets/Runes/efficiency.png",
            Tiers = CreateRuneTiers("Offensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune des Materie-Segens",
            Category = "Sonstige",
            ImagePath = "/Assets/Runes/matter_blessing.png",
            Tiers = CreateRuneTiers("Sonstige")
        },
        new RuneCollection
        {
            RuneName = "Rune des Sphären-Veränderers",
            Category = "Defensiv",
            ImagePath = "/Assets/Runes/sphere_changer.png",
            Tiers = CreateRuneTiers("Defensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune des Panikhüters",
            Category = "Defensiv",
            ImagePath = "/Assets/Runes/panic_keeper.png",
            Tiers = CreateRuneTiers("Defensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune der Beständigkeit",
            Category = "Defensiv",
            ImagePath = "/Assets/Runes/steadfastness.png",
            Tiers = CreateRuneTiers("Defensiv")
        },
        new RuneCollection
        {
            RuneName = "Rune des Willens zur Macht",
            Category = "Sonstige",
            ImagePath = "/Assets/Runes/will_to_power.png",
            Tiers = CreateRuneTiers("Sonstige")
        },
        new RuneCollection
        {
            RuneName = "Rune der wachsenden Lebenskraft",
            Category = "Sonstige",
            ImagePath = "/Assets/Runes/growing_vitality.png",
            Tiers = CreateRuneTiers("Sonstige")
        }
        ];

        foreach (RuneCollection collection in collections)
            collection.AttachTierNotifications();

        return collections;
    }

    public static VarnokPlan CreateVarnokPlan()
    {
        return new VarnokPlan
        {
            Items =
            [
                new() { Name = "Helm", DreamRequired = 50, StarRequired = 5 },
                new() { Name = "Schultern", DreamRequired = 50, StarRequired = 5 },
                new() { Name = "Torso", DreamRequired = 50, StarRequired = 5 },
                new() { Name = "Handschuhe", DreamRequired = 50, StarRequired = 5 },
                new() { Name = "Schuhe", DreamRequired = 50, StarRequired = 5 },
                new() { Name = "Mantel", DreamRequired = 50, StarRequired = 5 },
                new() { Name = "GB-Waffe", DreamRequired = 100, StarRequired = 10 },
                new() { Name = "Pet", DreamRequired = 0, StarRequired = 20 },
                new() { Name = "Rune", DreamRequired = 0, StarRequired = 20 },
                new() { Name = "Teppich", DreamRequired = 100, StarRequired = 20 }
            ]
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

    public static void NormalizeCharacter(CharacterProfile character)
    {
        character.Builds ??= [];
        character.Gems ??= [];
        character.Runes ??= [];
        character.Mortis ??= CreateMortisPlan();
        character.Varnok ??= CreateVarnokPlan();

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

        Dictionary<string, string> tierAliases = new()
        {
            ["Trapez Verfeinert"] = "Trapez Verfeinerter",
            ["Trapez Brillant"] = "Trapez Brillanter",
            ["Imperial Verfeinert"] = "Imperial Verfeinerter",
            ["Imperial Brillant"] = "Imperial Brillanter"
        };

        foreach (GemCollection gem in character.Gems)
        {
            foreach (GemTierEntry tier in gem.Tiers)
            {
                if (tierAliases.TryGetValue(tier.TierName, out string? correctedName))
                    tier.TierName = correctedName;
            }
        }

        List<GemCollection> defaultGems = CreateGemCollections();
        foreach (GemCollection defaultGem in defaultGems)
        {
            GemCollection? existing = character.Gems.FirstOrDefault(x => x.GemName == defaultGem.GemName);
            if (existing is null)
            {
                character.Gems.Add(defaultGem);
                continue;
            }

            existing.ColorName = defaultGem.ColorName;
            existing.ColorHex = defaultGem.ColorHex;
            existing.Category = defaultGem.Category;
            existing.ImagePath = defaultGem.ImagePath;
            existing.Tiers ??= [];

            foreach (GemTierEntry defaultTier in defaultGem.Tiers)
            {
                GemTierEntry? tier = existing.Tiers.FirstOrDefault(x => x.TierName == defaultTier.TierName);
                if (tier is null)
                    existing.Tiers.Add(defaultTier);
                else
                {
                    tier.DustCost = defaultTier.DustCost;
                    tier.GoldCost = defaultTier.GoldCost;
                }
            }

            existing.Tiers = GetGemTiers(existing.Category)
                .Select(definition => existing.Tiers.First(x => x.TierName == definition.Name))
                .ToList();
            existing.AttachTierNotifications();
        }

        List<RuneCollection> defaultRunes = CreateRuneCollections();
        foreach (RuneCollection defaultRune in defaultRunes)
        {
            RuneCollection? existing = character.Runes.FirstOrDefault(x => x.RuneName == defaultRune.RuneName);
            if (existing is null)
            {
                character.Runes.Add(defaultRune);
                continue;
            }

            existing.Category = defaultRune.Category;
            existing.ImagePath = defaultRune.ImagePath;
            existing.Tiers ??= [];

            foreach (RuneTierEntry defaultTier in defaultRune.Tiers)
            {
                RuneTierEntry? tier = existing.Tiers.FirstOrDefault(x => x.TierName == defaultTier.TierName);
                if (tier is null)
                    existing.Tiers.Add(defaultTier);
                else
                {
                    tier.DustCost = defaultTier.DustCost;
                    tier.GoldCost = defaultTier.GoldCost;
                }
            }

            existing.AttachTierNotifications();
        }

        if (character.Mortis.Activities is null || character.Mortis.Activities.Count == 0)
            character.Mortis = CreateMortisPlan();

        if (character.Varnok.Items is null || character.Varnok.Items.Count == 0)
        {
            character.Varnok = CreateVarnokPlan();
        }
        else
        {
            VarnokPlan defaults = CreateVarnokPlan();

            foreach (VarnokItem defaultItem in defaults.Items)
            {
                VarnokItem? existing =
                    character.Varnok.Items.FirstOrDefault(x => x.Name == defaultItem.Name);

                if (existing is null)
                {
                    character.Varnok.Items.Add(defaultItem);
                    continue;
                }

                existing.DreamRequired = defaultItem.DreamRequired;
                existing.StarRequired = defaultItem.StarRequired;
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
