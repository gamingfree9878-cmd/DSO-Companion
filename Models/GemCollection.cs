using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DSOCompanion.Models;

public sealed class GemCollection : INotifyPropertyChanged
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

    [JsonIgnore]
    public int TotalQuantity => Tiers.Sum(x => x.Quantity);

    [JsonIgnore]
    public int MaximumQuantity =>
        Tiers.FirstOrDefault(x => x.TierName == "Maximal")?.Quantity ?? 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void AttachTierNotifications()
    {
        foreach (GemTierEntry tier in Tiers)
        {
            tier.PropertyChanged -= TierOnPropertyChanged;
            tier.PropertyChanged += TierOnPropertyChanged;
        }

        RaiseTotals();
    }

    private void TierOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GemTierEntry.Quantity))
            RaiseTotals();
    }

    private void RaiseTotals()
    {
        OnPropertyChanged(nameof(TotalDust));
        OnPropertyChanged(nameof(TotalGold));
        OnPropertyChanged(nameof(TotalQuantity));
        OnPropertyChanged(nameof(MaximumQuantity));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
