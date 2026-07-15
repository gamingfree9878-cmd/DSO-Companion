using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DSOCompanion.Models;
using DSOCompanion.Services;

namespace DSOCompanion;

public partial class MainWindow : Window
{
    private readonly StorageService _storage = new();
    private AppState _state;
    private bool _loading;
    private EquipmentSlot? _selectedEquipmentSlot;
    private string _activeGemFilter = "Alle";

    private CharacterProfile ActiveCharacter =>
        _state.Characters.First(x => x.Id == _state.ActiveCharacterId);

    private BuildProfile ActiveBuild =>
        ActiveCharacter.Builds.First(x => x.Id == ActiveCharacter.ActiveBuildId);

    public MainWindow()
    {
        InitializeComponent();

        ClassCombo.ItemsSource = AppDataService.Classes;
        ServerCombo.ItemsSource = AppDataService.Servers;
        BuildTypeCombo.ItemsSource = AppDataService.BuildTypes;

        _state = _storage.Load();
        NormalizeState();
        RefreshAll();
        ShowPage(DashboardPage, DashboardButton);
    }

    private void NormalizeState()
    {
        if (_state.Characters.Count == 0)
        {
            CharacterProfile character = AppDataService.CreateCharacter();
            _state.Characters.Add(character);
            _state.ActiveCharacterId = character.Id;
        }

        if (_state.ActiveCharacterId is null ||
            _state.Characters.All(x => x.Id != _state.ActiveCharacterId))
        {
            _state.ActiveCharacterId = _state.Characters[0].Id;
        }

        foreach (CharacterProfile character in _state.Characters)
            AppDataService.NormalizeCharacter(character);
    }

    private void RefreshAll()
    {
        _loading = true;

        CharacterCombo.ItemsSource = null;
        CharacterCombo.ItemsSource = _state.Characters;
        CharacterCombo.SelectedItem = ActiveCharacter;

        CharacterNameBox.Text = ActiveCharacter.Name;
        ClassCombo.SelectedItem = ActiveCharacter.CharacterClass;
        LevelBox.Text = ActiveCharacter.Level.ToString();
        ServerCombo.SelectedItem = ActiveCharacter.Server;
        CharacterNotesBox.Text = ActiveCharacter.Notes;

        BuildList.ItemsSource = null;
        BuildList.ItemsSource = ActiveCharacter.Builds;
        BuildList.SelectedItem = ActiveBuild;
        BuildTypeCombo.SelectedItem = ActiveBuild.BuildType;

        ActiveBuildHeader.Text = $"{ActiveBuild.Name} · {ActiveBuild.BuildType}";

        EquipmentSlotList.ItemsSource = null;
        EquipmentSlotList.ItemsSource = ActiveBuild.Equipment;
        EquipmentSlotList.SelectedIndex = 0;

        OwnedGemDustBox.Text = ActiveCharacter.OwnedGemDust.ToString();
        OwnedGoldBox.Text = ActiveCharacter.OwnedGold.ToString();

        _loading = false;

        LoadSelectedEquipmentSlot();
        RefreshGemPage();
        UpdateSummary();
        Save();
    }

    private void UpdateSummary()
    {
        SummaryCharacterText.Text = ActiveCharacter.Name;
        SummaryClassText.Text =
            $"{ActiveCharacter.CharacterClass} · {ActiveCharacter.Server} · Level {ActiveCharacter.Level}";
        SummaryBuildCountText.Text = ActiveCharacter.Builds.Count.ToString();

        DashboardCharacterText.Text = ActiveCharacter.Name;
        DashboardBuildText.Text = $"{ActiveBuild.Name} · {ActiveBuild.BuildType}";
        DashboardBuildCountText.Text = ActiveCharacter.Builds.Count.ToString();

        ActiveBuildHeader.Text = $"{ActiveBuild.Name} · {ActiveBuild.BuildType}";
    }

    private void Save()
    {
        _storage.Save(_state);
    }

    private void ShowPage(UIElement page, Button activeButton)
    {
        DashboardPage.Visibility = Visibility.Collapsed;
        CharactersPage.Visibility = Visibility.Collapsed;
        EquipmentPage.Visibility = Visibility.Collapsed;
        GemsPage.Visibility = Visibility.Collapsed;
        RunesPage.Visibility = Visibility.Collapsed;
        JewelsPage.Visibility = Visibility.Collapsed;
        MortisPage.Visibility = Visibility.Collapsed;

        page.Visibility = Visibility.Visible;

        Brush normal = (Brush)FindResource("Panel2Brush");
        Brush accent = (Brush)FindResource("AccentBrush");

        DashboardButton.Background = normal;
        CharactersButton.Background = normal;
        EquipmentButton.Background = normal;
        GemsButton.Background = normal;
        RunesButton.Background = normal;
        JewelsButton.Background = normal;
        MortisButton.Background = normal;

        activeButton.Background = accent;
    }

    private void LoadSelectedEquipmentSlot()
    {
        _selectedEquipmentSlot = EquipmentSlotList.SelectedItem as EquipmentSlot;

        _loading = true;

        SelectedSlotHeader.Text = _selectedEquipmentSlot?.SlotName ?? "Ausrüstung";
        ItemNameBox.Text = _selectedEquipmentSlot?.ItemName ?? "";
        ItemLevelBox.Text = (_selectedEquipmentSlot?.ItemLevel ?? 0).ToString();
        BaseValuesBox.Text = _selectedEquipmentSlot?.BaseValues ?? "";
        EnchantmentsBox.Text = _selectedEquipmentSlot?.Enchantments ?? "";
        GemsBox.Text = _selectedEquipmentSlot?.Gems ?? "";
        RunesBox.Text = _selectedEquipmentSlot?.Runes ?? "";
        JewelBox.Text = _selectedEquipmentSlot?.Jewel ?? "";
        EquipmentNotesBox.Text = _selectedEquipmentSlot?.Notes ?? "";

        _loading = false;
    }

    private void SaveSelectedEquipmentSlot()
    {
        if (_loading || _selectedEquipmentSlot is null)
            return;

        _selectedEquipmentSlot.ItemName = ItemNameBox.Text;
        _selectedEquipmentSlot.ItemLevel =
            int.TryParse(ItemLevelBox.Text, out int level)
                ? Math.Max(0, level)
                : 0;

        _selectedEquipmentSlot.BaseValues = BaseValuesBox.Text;
        _selectedEquipmentSlot.Enchantments = EnchantmentsBox.Text;
        _selectedEquipmentSlot.Gems = GemsBox.Text;
        _selectedEquipmentSlot.Runes = RunesBox.Text;
        _selectedEquipmentSlot.Jewel = JewelBox.Text;
        _selectedEquipmentSlot.Notes = EquipmentNotesBox.Text;

        Save();
    }

    private void RefreshGemPage()
    {
        IEnumerable<GemCollection> filtered = _activeGemFilter switch
        {
            "Offensiv" => ActiveCharacter.Gems.Where(x => string.Equals(x.Category, "Offensiv", StringComparison.OrdinalIgnoreCase)),
            "Defensiv" => ActiveCharacter.Gems.Where(x => string.Equals(x.Category, "Defensiv", StringComparison.OrdinalIgnoreCase)),
            "Diamanten" => ActiveCharacter.Gems.Where(x => string.Equals(x.Category, "Diamanten", StringComparison.OrdinalIgnoreCase)),
            "Opal" => ActiveCharacter.Gems.Where(x => string.Equals(x.Category, "Opal", StringComparison.OrdinalIgnoreCase)),
            _ => ActiveCharacter.Gems
        };

        GemCardsControl.ItemsSource = null;
        GemCardsControl.ItemsSource = filtered.ToList();

        UpdateGemTotals();
        UpdateGemFilterButtons();
    }

    private void UpdateGemTotals()
    {
        long dust = ActiveCharacter.Gems
            .SelectMany(x => x.Tiers)
            .Sum(x => (long)x.DustCost * x.Quantity);

        long gold = ActiveCharacter.Gems
            .SelectMany(x => x.Tiers)
            .Sum(x => (long)x.GoldCost * x.Quantity);

        long missingDust = Math.Max(0, dust - ActiveCharacter.OwnedGemDust);
        long missingGold = Math.Max(0, gold - ActiveCharacter.OwnedGold);

        GemNeededDustText.Text = dust.ToString("N0");
        GemMissingDustText.Text = missingDust.ToString("N0");
        GemNeededGoldText.Text = gold.ToString("N0");
        GemMissingGoldText.Text = missingGold.ToString("N0");

        GemCardsControl.Items.Refresh();
        Save();
    }

    private void UpdateGemFilterButtons()
    {
        Brush normal = (Brush)FindResource("Panel2Brush");
        Brush accent = (Brush)FindResource("AccentBrush");

        GemFilterAllButton.Background = normal;
        GemFilterOffensiveButton.Background = normal;
        GemFilterDefensiveButton.Background = normal;
        GemFilterOpalButton.Background = normal;
        GemFilterDiamondsButton.Background = normal;

        Button active = _activeGemFilter switch
        {
            "Offensiv" => GemFilterOffensiveButton,
            "Defensiv" => GemFilterDefensiveButton,
            "Opal" => GemFilterOpalButton,
            "Diamanten" => GemFilterDiamondsButton,
            _ => GemFilterAllButton
        };

        active.Background = accent;
    }

    private void GemResource_OnChanged(object sender, TextChangedEventArgs e)
    {
        if (_loading)
            return;

        ActiveCharacter.OwnedGemDust =
            int.TryParse(OwnedGemDustBox.Text, out int dust)
                ? Math.Max(0, dust)
                : 0;

        ActiveCharacter.OwnedGold =
            int.TryParse(OwnedGoldBox.Text, out int gold)
                ? Math.Max(0, gold)
                : 0;

        UpdateGemTotals();
    }

    private void GemCardPlus_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is GemTierEntry tier)
        {
            tier.Quantity++;
            UpdateGemTotals();
        }
    }

    private void GemCardMinus_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is GemTierEntry tier)
        {
            tier.Quantity = Math.Max(0, tier.Quantity - 1);
            UpdateGemTotals();
        }
    }

    private void ClearAllGems_OnClick(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(
                "Alle Edelsteinmengen dieses Charakters auf 0 setzen?",
                "Bestätigung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        foreach (GemTierEntry tier in ActiveCharacter.Gems.SelectMany(x => x.Tiers))
            tier.Quantity = 0;

        UpdateGemTotals();
    }

    private void SetGemFilter(string filter)
    {
        _activeGemFilter = filter;
        RefreshGemPage();
    }

    private void GemFilterAll_OnClick(object sender, RoutedEventArgs e) =>
        SetGemFilter("Alle");

    private void GemFilterOffensive_OnClick(object sender, RoutedEventArgs e) =>
        SetGemFilter("Offensiv");

    private void GemFilterDefensive_OnClick(object sender, RoutedEventArgs e) =>
        SetGemFilter("Defensiv");

    private void GemFilterOpal_OnClick(object sender, RoutedEventArgs e) =>
        SetGemFilter("Opal");

    private void GemFilterDiamonds_OnClick(object sender, RoutedEventArgs e) =>
        SetGemFilter("Diamanten");

    private static string? Prompt(string title, string initialValue)
    {
        Window dialog = new()
        {
            Title = title,
            Width = 420,
            Height = 170,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = Brushes.Black,
            Foreground = Brushes.White
        };

        StackPanel panel = new() { Margin = new Thickness(16) };
        TextBox input = new()
        {
            Text = initialValue,
            Margin = new Thickness(0, 0, 0, 12)
        };

        StackPanel buttons = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        Button ok = new()
        {
            Content = "OK",
            IsDefault = true,
            MinWidth = 80
        };

        Button cancel = new()
        {
            Content = "Abbrechen",
            IsCancel = true,
            MinWidth = 90
        };

        string? result = null;

        ok.Click += (_, _) =>
        {
            result = input.Text;
            dialog.DialogResult = true;
        };

        buttons.Children.Add(ok);
        buttons.Children.Add(cancel);
        panel.Children.Add(input);
        panel.Children.Add(buttons);
        dialog.Content = panel;

        dialog.ShowDialog();
        return result;
    }

    private void DashboardButton_OnClick(object sender, RoutedEventArgs e) =>
        ShowPage(DashboardPage, DashboardButton);

    private void CharactersButton_OnClick(object sender, RoutedEventArgs e) =>
        ShowPage(CharactersPage, CharactersButton);

    private void EquipmentButton_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshAll();
        ShowPage(EquipmentPage, EquipmentButton);
    }

    private void GemsButton_OnClick(object sender, RoutedEventArgs e) =>
        ShowPage(GemsPage, GemsButton);

    private void RunesButton_OnClick(object sender, RoutedEventArgs e) =>
        ShowPage(RunesPage, RunesButton);

    private void JewelsButton_OnClick(object sender, RoutedEventArgs e) =>
        ShowPage(JewelsPage, JewelsButton);

    private void MortisButton_OnClick(object sender, RoutedEventArgs e) =>
        ShowPage(MortisPage, MortisButton);

    private void CharacterCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || CharacterCombo.SelectedItem is not CharacterProfile character)
            return;

        _state.ActiveCharacterId = character.Id;
        RefreshAll();
    }

    private void AddCharacter_OnClick(object sender, RoutedEventArgs e)
    {
        string? name = Prompt("Neuen Charakter anlegen", "Neuer Charakter");

        if (string.IsNullOrWhiteSpace(name))
            return;

        CharacterProfile character = AppDataService.CreateCharacter(name);
        _state.Characters.Add(character);
        _state.ActiveCharacterId = character.Id;
        RefreshAll();
    }

    private void RenameCharacter_OnClick(object sender, RoutedEventArgs e)
    {
        string? name = Prompt("Charakter umbenennen", ActiveCharacter.Name);

        if (string.IsNullOrWhiteSpace(name))
            return;

        ActiveCharacter.Name = name;
        RefreshAll();
    }

    private void DeleteCharacter_OnClick(object sender, RoutedEventArgs e)
    {
        if (_state.Characters.Count == 1)
        {
            MessageBox.Show("Mindestens ein Charakter muss bestehen bleiben.");
            return;
        }

        if (MessageBox.Show(
                "Diesen Charakter wirklich löschen?",
                "Bestätigung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        _state.Characters.Remove(ActiveCharacter);
        _state.ActiveCharacterId = _state.Characters[0].Id;
        RefreshAll();
    }

    private void CharacterField_OnChanged(object sender, EventArgs e)
    {
        if (_loading)
            return;

        ActiveCharacter.Name = CharacterNameBox.Text;
        ActiveCharacter.CharacterClass =
            ClassCombo.SelectedItem?.ToString() ?? "Zirkelmagier";

        ActiveCharacter.Level =
            int.TryParse(LevelBox.Text, out int level)
                ? Math.Max(1, level)
                : 1;

        ActiveCharacter.Server =
            ServerCombo.SelectedItem?.ToString() ?? "Heredur";

        ActiveCharacter.Notes = CharacterNotesBox.Text;

        CharacterCombo.Items.Refresh();
        UpdateSummary();
        Save();
    }

    private void BuildList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || BuildList.SelectedItem is not BuildProfile build)
            return;

        SaveSelectedEquipmentSlot();
        ActiveCharacter.ActiveBuildId = build.Id;
        RefreshAll();
    }

    private void BuildTypeCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading)
            return;

        ActiveBuild.BuildType =
            BuildTypeCombo.SelectedItem?.ToString() ?? "Eigen";

        UpdateSummary();
        Save();
    }

    private void AddBuild_OnClick(object sender, RoutedEventArgs e)
    {
        string? name = Prompt("Neuen Build anlegen", "Neuer Build");

        if (string.IsNullOrWhiteSpace(name))
            return;

        BuildProfile build = AppDataService.CreateBuild(name, "Eigen");
        ActiveCharacter.Builds.Add(build);
        ActiveCharacter.ActiveBuildId = build.Id;
        RefreshAll();
    }

    private void RenameBuild_OnClick(object sender, RoutedEventArgs e)
    {
        if (BuildList.SelectedItem is not BuildProfile build)
            return;

        string? name = Prompt("Build umbenennen", build.Name);

        if (string.IsNullOrWhiteSpace(name))
            return;

        build.Name = name;
        RefreshAll();
    }

    private void CopyBuild_OnClick(object sender, RoutedEventArgs e)
    {
        if (BuildList.SelectedItem is not BuildProfile build)
            return;

        string? name = Prompt("Build kopieren", build.Name + " Kopie");

        if (string.IsNullOrWhiteSpace(name))
            return;

        BuildProfile copy = new()
        {
            Name = name,
            BuildType = build.BuildType,
            Equipment = build.Equipment
                .Select(item => new EquipmentSlot
                {
                    SlotName = item.SlotName,
                    ItemName = item.ItemName,
                    ItemLevel = item.ItemLevel,
                    BaseValues = item.BaseValues,
                    Enchantments = item.Enchantments,
                    Gems = item.Gems,
                    Runes = item.Runes,
                    Jewel = item.Jewel,
                    Notes = item.Notes
                })
                .ToList()
        };

        ActiveCharacter.Builds.Add(copy);
        ActiveCharacter.ActiveBuildId = copy.Id;
        RefreshAll();
    }

    private void DeleteBuild_OnClick(object sender, RoutedEventArgs e)
    {
        if (ActiveCharacter.Builds.Count == 1)
        {
            MessageBox.Show("Mindestens ein Build muss bestehen bleiben.");
            return;
        }

        if (BuildList.SelectedItem is not BuildProfile build)
            return;

        if (MessageBox.Show(
                "Diesen Build wirklich löschen?",
                "Bestätigung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        ActiveCharacter.Builds.Remove(build);
        ActiveCharacter.ActiveBuildId = ActiveCharacter.Builds[0].Id;
        RefreshAll();
    }

    private void EquipmentSlotList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading)
            return;

        SaveSelectedEquipmentSlot();
        LoadSelectedEquipmentSlot();
    }

    private void EquipmentField_OnChanged(object sender, TextChangedEventArgs e)
    {
        SaveSelectedEquipmentSlot();
    }
}
