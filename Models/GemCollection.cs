namespace DSOCompanion.Models;

public sealed class GemCollection
{
    public string GemName { get; set; } = "";
    public string ColorHex { get; set; } = "#8061F1";
    public List<GemTierEntry> Tiers { get; set; } = [];
}
