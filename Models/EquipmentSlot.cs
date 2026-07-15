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
}
