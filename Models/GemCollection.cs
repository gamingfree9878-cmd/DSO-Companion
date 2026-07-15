using System.Text.Json.Serialization;

namespace DSOCompanion.Models;

public sealed class GemCollection
{
    public string GemName { get; set; } = "";
    public string ColorName { get; set; } = "";
    public string ColorHex { get; set; } = "#8061F1";
    public string Category { get; set; } = "Offensiv";

    [JsonIgnore]
    public string ImagePath { get; set; } = "";
    public List<GemTierEntry> Tiers { get; set; } = [];

    [JsonIgnore]
    public long TotalDust => Tiers.Sum(x => (long)x.DustCost * x.Quantity);

    [JsonIgnore]
    public long TotalGold => Tiers.Sum(x => (long)x.GoldCost * x.Quantity);
}
