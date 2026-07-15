namespace DSOCompanion.Models;

public sealed class GemTierEntry
{
    public string TierName { get; set; } = "";
    public int DustCost { get; set; }
    public int GoldCost { get; set; }
    public int Quantity { get; set; }
}
