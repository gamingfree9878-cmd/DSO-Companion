namespace DSOCompanion.Models;

public sealed class EquipmentSlot
{
    public string SlotName { get; set; } = "";
    public string ItemName { get; set; } = "";
    public int ItemLevel { get; set; }
    public string BaseValues { get; set; } = "";
    public string Enchantments { get; set; } = "";
    public string Gems { get; set; } = "";
    public string Runes { get; set; } = "";
    public string Jewel { get; set; } = "";
    public string Notes { get; set; } = "";
    public string Rarity { get; set; } = "";
    public string ItemType { get; set; } = "";
    public string SetBonus { get; set; } = "";
    public string ObtainableFrom { get; set; } = "";
    public string SourceUrl { get; set; } = "";
}
