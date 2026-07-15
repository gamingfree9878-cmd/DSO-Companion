using DSOCompanion.Models;

namespace DSOCompanion.Services;

public static class ItemDatabaseService
{
    public static IReadOnlyList<DsoItem> Items { get; } =
    [
        new DsoItem
        {
            Name = "Grimace of the Immortal Beast",
            CharacterClass = "Zirkelmagier",
            SlotName = "Waffe",
            ItemType = "Zweihandstab",
            Rarity = "Mythisch",
            ItemLevel = 145,
            BaseValues =
                "+ 26.326 Schaden\n" +
                "+ 3.506 kritischer Wert",
            UniqueValues =
                "+ 0,102 Angriffsgeschwindigkeit auf diesem Gegenstand\n" +
                "+ 10,00 % Schaden\n\n" +
                "Besiegte Gegner können eine kreuzförmige Aura auslösen, " +
                "die abhängig von der Entfernung zum Zentrum bis zu 250 % " +
                "des Basisschadens verursacht. Der Schadenstyp richtet sich " +
                "nach der gewählten Elementarbeherrschung.\n\n" +
                "Unter 33 % Lebenspunkten kann Wind der Erholung ausgelöst werden: " +
                "5 Sekunden lang werden pro Sekunde 1 % der maximalen Lebenspunkte " +
                "regeneriert; bis zu 3 Stapel.",
            ObtainableFrom = "Dracanisches Jubiläumsfest",
            SourceUrl = "https://dsofun.com/en/grimace-of-the-immortal-beast/"
        },
        new DsoItem
        {
            Name = "Old Glory",
            CharacterClass = "Zirkelmagier",
            SlotName = "Umhang",
            ItemType = "Umhang",
            Rarity = "Mythisch",
            ItemLevel = 145,
            BaseValues =
                "+ 1.892 Schaden\n" +
                "+ 0,060 Angriffe pro Sekunde\n" +
                "+ 16.507 Lebenspunkte",
            UniqueValues = "+ 10,00 % Schaden",
            SetBonus =
                "Forgotten Glory (2 Teile): Bei jeder Benutzung von Ice Missile " +
                "werden 3 Projektile abgefeuert.",
            ObtainableFrom =
                "Big Game Hunt – Blazing Inferno, alle Bosse und Gruppenwächter",
            SourceUrl = "https://dsofun.com/en/old-glory-sw/"
        },
        new DsoItem
        {
            Name = "Kaylin Lefrye's Cloak",
            CharacterClass = "Zirkelmagier",
            SlotName = "Umhang",
            ItemType = "Umhang",
            Rarity = "Einzigartig",
            ItemLevel = 145,
            BaseValues =
                "+ 1.135–1.892 Schaden\n" +
                "+ 842–2.104 kritischer Wert\n" +
                "+ 2.475–16.507 Lebenspunkte",
            UniqueValues =
                "+ 1.624 kritischer Wert auf diesem Gegenstand\n" +
                "+ 7 Mana",
            ObtainableFrom = "Werkbank",
            SourceUrl = "https://dsofun.com/en/kaylin-lefryes-cloak-sw/"
        },
        new DsoItem
        {
            Name = "Kraken",
            CharacterClass = "Zirkelmagier",
            SlotName = "Waffe",
            ItemType = "Zweihandstab",
            Rarity = "Einzigartig",
            ItemLevel = 145,
            BaseValues =
                "+ 3.948–6.582 Schaden\n" +
                "0,132–0,329 Angriffe pro Sekunde",
            UniqueValues =
                "+ 94,322 % Waffenangriffsgeschwindigkeit auf diesem Gegenstand",
            ObtainableFrom =
                "Entweihte Zuflucht – Gorga (Stufe 140) oder Werkbank (Stufe 145)",
            SourceUrl = "https://dsofun.com/en/kraken/"
        }
    ];

    public static List<DsoItem> GetForClass(string characterClass)
    {
        return Items
            .Where(item => item.CharacterClass == characterClass)
            .OrderBy(item => item.SlotName)
            .ThenBy(item => item.Name)
            .ToList();
    }
}
