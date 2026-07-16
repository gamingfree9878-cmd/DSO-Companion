using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DSOCompanion.Models;

public sealed class RuneTierEntry : INotifyPropertyChanged
{
    private int _quantity;

    public string TierName { get; set; } = "";
    public int DustCost { get; set; }
    public int GoldCost { get; set; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            int normalized = Math.Max(0, value);
            if (_quantity == normalized)
                return;

            _quantity = normalized;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
