using System.Text.Json.Serialization;

namespace DSOCompanion.Models;

public sealed class RuneCollection
{
    public string RuneName { get; set; } = "";
    public string Category { get; set; } = "Offensiv";

    [JsonIgnore]
    public string ImagePath { get; set; } = "";

    public List<RuneTierEntry> Tiers { get; set; } = [];

    [JsonIgnore]
    public long TotalDust => Tiers.Sum(x => (long)x.DustCost * x.Quantity);

    [JsonIgnore]
    public long TotalGold => Tiers.Sum(x => (long)x.GoldCost * x.Quantity);
}
