using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DSOCompanion.Models;
using DSOCompanion.Services;
using Microsoft.Win32;

namespace DSOCompanion;

public partial class MainWindow : Window
{
    private readonly StorageService _storage = new();
    private AppState _state;
    private bool _loading;
    private EquipmentSlot? _selectedSlot;

    private CharacterProfile ActiveProfile =>
        _state.Profiles.First(x => x.Id == _state.ActiveProfileId);

    private BuildProfile ActiveBuild =>
        ActiveProfile.Builds.First(x => x.Id == ActiveProfile.ActiveBuildId);

    public MainWindow()
    {
        InitializeComponent();
        _state = _storage.Load();
        NormalizeState();
        RefreshAll();
    }

    private void NormalizeState()
    {
        if (_state.Profiles.Count == 0)
        {
            var profile = DefaultDataService.CreateCharacter();
            _state.Profiles.Add(profile);
            _state.ActiveProfileId = profile.Id;
        }

        foreach (var profile in _state.Profiles)
        {
            if (profile.Builds.Count == 0)
            {
                var build = DefaultDataService.CreateBuild("Boss-Build");
                profile.Builds.Add(build);
                profile.ActiveBuildId = build.Id;
            }

            if (profile.ActiveBuildId is null ||
                profile.Builds.All(x => x.Id != profile.ActiveBuildId))
            {
                profile.ActiveBuildId = profile.Builds[0].Id;
            }
        }

        if (_state.ActiveProfileId is null ||
            _state.Profiles.All(x => x.Id != _state.ActiveProfileId))
        {
            _state.ActiveProfileId = _state.Profiles[0].Id;
        }

        if (_state.Mortis.Activities.Count == 0)
            _state.Mortis = DefaultDataService.CreateMortisPlan();
    }

    private void RefreshAll()
    {
        _loading = true;

        ProfileCombo.ItemsSource = null;
        ProfileCombo.ItemsSource = _state.Profiles;
        ProfileCombo.SelectedItem = ActiveProfile;

        ClassCombo.SelectedIndex = ActiveProfile.CharacterClass switch
        {
            "Drachenkrieger" => 1,
            "Waldläufer" => 2,
            "Dampfmechanikus" => 3,
            _ => 0
        };

        BuildCombo.ItemsSource = null;
        BuildCombo.ItemsSource = ActiveProfile.Builds;
        BuildCombo.SelectedItem = ActiveBuild;

        SlotList.ItemsSource = null;
        SlotList.ItemsSource = ActiveBuild.Equipment;
        SlotList.SelectedIndex = 0;

        MortisGrid.ItemsSource = null;
        MortisGrid.ItemsSource = _state.Mortis.Activities;

        TargetBonesBox.Text = _state.Mortis.TargetBones.ToString();
        CurrentBonesBox.Text = _state.Mortis.CurrentBones.ToString();
        DaysLeftBox.Text = _state.Mortis.DaysLeft.ToString();
        HoursPerDayBox.Text = _state.Mortis.HoursPerDay.ToString();
        MinutesPerRunBox.Text = _state.Mortis.MinutesPerRun.ToString();

        _loading = false;
        LoadSelectedSlot();
        UpdateDashboard();
        UpdateMortisSummary();
        Save();
    }

    private void LoadSelectedSlot()
    {
        _selectedSlot = SlotList.SelectedItem as EquipmentSlot;
        _loading = true;

        ItemNameBox.Text = _selectedSlot?.ItemName ?? "";
        ItemLevelBox.Text = (_selectedSlot?.ItemLevel ?? 0).ToString();
        BaseValuesBox.Text = _selectedSlot?.BaseValues ?? "";
        EnchantmentsBox.Text = _selectedSlot?.Enchantments ?? "";
        GemsBox.Text = _selectedSlot?.Gems ?? "";
        RunesBox.Text = _selectedSlot?.Runes ?? "";
        JewelBox.Text = _selectedSlot?.Jewel ?? "";
        NotesBox.Text = _selectedSlot?.Notes ?? "";

        _loading = false;
    }

    private void SaveSelectedSlot()
    {
        if (_loading || _selectedSlot is null)
            return;

        _selectedSlot.ItemName = ItemNameBox.Text;
        _selectedSlot.ItemLevel = int.TryParse(ItemLevelBox.Text, out var level) ? level : 0;
        _selectedSlot.BaseValues = BaseValuesBox.Text;
        _selectedSlot.Enchantments = EnchantmentsBox.Text;
        _selectedSlot.Gems = GemsBox.Text;
        _selectedSlot.Runes = RunesBox.Text;
        _selectedSlot.Jewel = JewelBox.Text;
        _selectedSlot.Notes = NotesBox.Text;

        Save();
    }

    private void UpdateDashboard()
    {
        DashboardTitle.Text = ActiveProfile.Name;
        DashboardSubtitle.Text = $"{ActiveProfile.CharacterClass} · Aktiver Build: {ActiveBuild.Name}";
        BuildCountText.Text = ActiveProfile.Builds.Count.ToString();
        DashboardBonesText.Text = _state.Mortis.FinalBones.ToString("N0");
        DashboardMissingText.Text = _state.Mortis.MissingBones.ToString("N0");
    }

    private void UpdateMortisSummary()
    {
        MortisGrid.Items.Refresh();
        var m = _state.Mortis;
        MortisSummaryText.Text =
            $"Geplante Knochen: {m.PlannedBones:N0}  |  Knochen danach: {m.FinalBones:N0}  |  Noch fehlend: {m.MissingBones:N0}\n" +
            $"Runs: {m.TotalRuns:N0}  |  Eingänge: {m.TotalEntries:N0}  |  Zeit: {m.TotalHours:N1} h  |  Zeitbudget: {m.TimeBudgetHours:N1} h  |  Pro Tag nötig: {m.BonesPerDay:N0}";
        UpdateDashboard();
        Save();
    }

    private void Save() => _storage.Save(_state);

    private static string? Prompt(string title, string initial)
    {
        var dialog = new Window
        {
            Title = title,
            Width = 420,
            Height = 160,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = System.Windows.Media.Brushes.Black
        };

        var panel = new StackPanel { Margin = new Thickness(15) };
        var box = new TextBox { Text = initial, Margin = new Thickness(0, 0, 0, 10) };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var ok = new Button { Content = "OK", IsDefault = true };
        var cancel = new Button { Content = "Abbrechen", IsCancel = true };

        string? result = null;
        ok.Click += (_, _) => { result = box.Text; dialog.DialogResult = true; };
        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        panel.Children.Add(box);
        panel.Children.Add(buttons);
        dialog.Content = panel;
        dialog.ShowDialog();
        return result;
    }

    private void ProfileCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || ProfileCombo.SelectedItem is not CharacterProfile profile)
            return;

        _state.ActiveProfileId = profile.Id;
        RefreshAll();
    }

    private void AddProfile_OnClick(object sender, RoutedEventArgs e)
    {
        var name = Prompt("Neues Profil", "Neuer Charakter");
        if (string.IsNullOrWhiteSpace(name)) return;

        var profile = DefaultDataService.CreateCharacter(name);
        _state.Profiles.Add(profile);
        _state.ActiveProfileId = profile.Id;
        RefreshAll();
    }

    private void RenameProfile_OnClick(object sender, RoutedEventArgs e)
    {
        var name = Prompt("Profil umbenennen", ActiveProfile.Name);
        if (string.IsNullOrWhiteSpace(name)) return;
        ActiveProfile.Name = name;
        RefreshAll();
    }

    private void DeleteProfile_OnClick(object sender, RoutedEventArgs e)
    {
        if (_state.Profiles.Count == 1)
        {
            MessageBox.Show("Mindestens ein Profil muss bestehen bleiben.");
            return;
        }

        if (MessageBox.Show("Profil wirklich löschen?", "Bestätigung", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        _state.Profiles.Remove(ActiveProfile);
        _state.ActiveProfileId = _state.Profiles[0].Id;
        RefreshAll();
    }

    private void ClassCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || ClassCombo.SelectedItem is not ComboBoxItem item) return;
        ActiveProfile.CharacterClass = item.Content?.ToString() ?? "Zirkelmagier";
        UpdateDashboard();
        Save();
    }

    private void BuildCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || BuildCombo.SelectedItem is not BuildProfile build) return;
        ActiveProfile.ActiveBuildId = build.Id;
        RefreshAll();
    }

    private void AddBuild_OnClick(object sender, RoutedEventArgs e)
    {
        var name = Prompt("Neuer Build", "Neuer Build");
        if (string.IsNullOrWhiteSpace(name)) return;

        var build = DefaultDataService.CreateBuild(name);
        ActiveProfile.Builds.Add(build);
        ActiveProfile.ActiveBuildId = build.Id;
        RefreshAll();
    }

    private void CopyBuild_OnClick(object sender, RoutedEventArgs e)
    {
        var name = Prompt("Build kopieren", ActiveBuild.Name + " Kopie");
        if (string.IsNullOrWhiteSpace(name)) return;

        var json = System.Text.Json.JsonSerializer.Serialize(ActiveBuild);
        var copy = System.Text.Json.JsonSerializer.Deserialize<BuildProfile>(json)!;
        copy.Id = Guid.NewGuid();
        copy.Name = name;
        ActiveProfile.Builds.Add(copy);
        ActiveProfile.ActiveBuildId = copy.Id;
        RefreshAll();
    }

    private void RenameBuild_OnClick(object sender, RoutedEventArgs e)
    {
        var name = Prompt("Build umbenennen", ActiveBuild.Name);
        if (string.IsNullOrWhiteSpace(name)) return;
        ActiveBuild.Name = name;
        RefreshAll();
    }

    private void DeleteBuild_OnClick(object sender, RoutedEventArgs e)
    {
        if (ActiveProfile.Builds.Count == 1)
        {
            MessageBox.Show("Mindestens ein Build muss bestehen bleiben.");
            return;
        }

        if (MessageBox.Show("Build wirklich löschen?", "Bestätigung", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        ActiveProfile.Builds.Remove(ActiveBuild);
        ActiveProfile.ActiveBuildId = ActiveProfile.Builds[0].Id;
        RefreshAll();
    }

    private void ExportBuild_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "DSO Build (*.dsobuild)|*.dsobuild",
            FileName = $"{ActiveProfile.Name}_{ActiveBuild.Name}.dsobuild"
        };

        if (dialog.ShowDialog() == true)
            _storage.ExportBuild(ActiveBuild, dialog.FileName);
    }

    private void ImportBuild_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "DSO Build (*.dsobuild)|*.dsobuild"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var build = _storage.ImportBuild(dialog.FileName);
            build.Id = Guid.NewGuid();
            ActiveProfile.Builds.Add(build);
            ActiveProfile.ActiveBuildId = build.Id;
            RefreshAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Import fehlgeschlagen");
        }
    }

    private void SlotList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading) return;
        LoadSelectedSlot();
    }

    private void EquipmentField_OnChanged(object sender, TextChangedEventArgs e) => SaveSelectedSlot();

    private void MortisInput_OnChanged(object sender, TextChangedEventArgs e)
    {
        if (_loading) return;

        _state.Mortis.TargetBones = int.TryParse(TargetBonesBox.Text, out var target) ? target : 0;
        _state.Mortis.CurrentBones = int.TryParse(CurrentBonesBox.Text, out var current) ? current : 0;
        _state.Mortis.DaysLeft = int.TryParse(DaysLeftBox.Text, out var days) ? days : 0;
        _state.Mortis.HoursPerDay = decimal.TryParse(HoursPerDayBox.Text, out var hours) ? hours : 0;
        _state.Mortis.MinutesPerRun = decimal.TryParse(MinutesPerRunBox.Text, out var minutes) ? minutes : 0;
        UpdateMortisSummary();
    }

    private void MortisGrid_OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(UpdateMortisSummary, DispatcherPriority.Background);
    }
}
