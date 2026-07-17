using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DSOCompanion.Models;

public sealed class VarnokItem : INotifyPropertyChanged
{
    private int _dreamOwned;
    private int _starOwned;

    public string Name { get; set; } = "";
    public int DreamRequired { get; set; }
    public int StarRequired { get; set; }

    public int DreamOwned
    {
        get => _dreamOwned;
        set
        {
            int normalized = Math.Max(0, value);
            if (_dreamOwned == normalized)
                return;

            _dreamOwned = normalized;
            NotifyAll();
        }
    }

    public int StarOwned
    {
        get => _starOwned;
        set
        {
            int normalized = Math.Max(0, value);
            if (_starOwned == normalized)
                return;

            _starOwned = normalized;
            NotifyAll();
        }
    }

    public int DreamMissing => Math.Max(0, DreamRequired - DreamOwned);
    public int StarMissing => Math.Max(0, StarRequired - StarOwned);

    public bool IsComplete =>
        DreamMissing == 0 &&
        StarMissing == 0;

    public double Progress
    {
        get
        {
            int required = DreamRequired + StarRequired;
            if (required <= 0)
                return 100;

            int counted =
                Math.Min(DreamOwned, DreamRequired) +
                Math.Min(StarOwned, StarRequired);

            return Math.Min(100, counted * 100.0 / required);
        }
    }

    public string StatusText =>
        IsComplete
            ? "✓ Fertig"
            : Progress >= 75
                ? "Fast fertig"
                : Progress > 0
                    ? "In Arbeit"
                    : "Noch offen";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void NotifyAll()
    {
        OnPropertyChanged(nameof(DreamOwned));
        OnPropertyChanged(nameof(StarOwned));
        OnPropertyChanged(nameof(DreamMissing));
        OnPropertyChanged(nameof(StarMissing));
        OnPropertyChanged(nameof(IsComplete));
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(StatusText));
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
