using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DSOCompanion.Models;

public sealed class MortisActivity : INotifyPropertyChanged
{
    private int _entries;
    private int _runs;

    public string Name { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public int BonesPerRun { get; set; }

    public int Entries
    {
        get => _entries;
        set
        {
            int normalized = Math.Max(0, value);
            if (_entries == normalized)
                return;
            _entries = normalized;
            OnPropertyChanged();
        }
    }

    public int Runs
    {
        get => _runs;
        set
        {
            int normalized = Math.Max(0, value);
            if (_runs == normalized)
                return;

            _runs = normalized;
            Entries = normalized;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TotalBones));
        }
    }

    public int TotalBones => BonesPerRun * Runs;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
