namespace DSOCompanion.Models;

public sealed class DsoItem
{
    public string Name { get; set; } = "";
    public string CharacterClass { get; set; } = "";
    public string SlotName { get; set; } = "";
    public string ItemType { get; set; } = "";
    public string Rarity { get; set; } = "";
    public int ItemLevel { get; set; }
    public string BaseValues { get; set; } = "";
    public string UniqueValues { get; set; } = "";
    public string SetBonus { get; set; } = "";
    public string ObtainableFrom { get; set; } = "";
    public string SourceUrl { get; set; } = "";

    public string Subtitle => $"{Rarity} · {SlotName} · Stufe {ItemLevel}";
}
