using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DSOCompanion.Models;

public sealed class RuneCollection : INotifyPropertyChanged
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

    [JsonIgnore]
    public int TotalQuantity => Tiers.Sum(x => x.Quantity);

    public event PropertyChangedEventHandler? PropertyChanged;

    public void AttachTierNotifications()
    {
        foreach (RuneTierEntry tier in Tiers)
        {
            tier.PropertyChanged -= TierOnPropertyChanged;
            tier.PropertyChanged += TierOnPropertyChanged;
        }

        RaiseTotals();
    }

    private void TierOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RuneTierEntry.Quantity))
            RaiseTotals();
    }

    private void RaiseTotals()
    {
        OnPropertyChanged(nameof(TotalDust));
        OnPropertyChanged(nameof(TotalGold));
        OnPropertyChanged(nameof(TotalQuantity));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
