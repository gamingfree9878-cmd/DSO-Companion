using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text.Json;
using Microsoft.Win32;
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
    private string _activeRuneFilter = "Alle";

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

        OwnedRuneDustBox.Text = ActiveCharacter.OwnedRuneDust.ToString();
        OwnedRuneGoldBox.Text = ActiveCharacter.OwnedRuneGold.ToString();

        MortisTargetBox.Text = ActiveCharacter.Mortis.TargetBones.ToString();
        MortisCurrentBox.Text = ActiveCharacter.Mortis.CurrentBones.ToString();
        MortisAndermantBox.Text = ActiveCharacter.Mortis.AndermantPerEntry.ToString();
        MortisCoinsBox.Text = ActiveCharacter.Mortis.MortisCoins.ToString();
        ElixirPlannedRunsBox.Text = ActiveCharacter.Mortis.ElixirPlannedRuns.ToString();
        RunsPerElixirBox.Text = ActiveCharacter.Mortis.RunsPerElixir.ToString();

        MortisCardsControl.ItemsSource = null;
        MortisCardsControl.ItemsSource = ActiveCharacter.Mortis.Activities;

        VarnokCardsControl.ItemsSource = null;
        VarnokCardsControl.ItemsSource = ActiveCharacter.Varnok.Items;

        _loading = false;

        LoadSelectedEquipmentSlot();
        RefreshGemPage();
        RefreshRunePage();
        UpdateMortisSummary();
        UpdateVarnokSummary();
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
        VarnokPage.Visibility = Visibility.Collapsed;
        LicensePage.Visibility = Visibility.Collapsed;

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
        VarnokButton.Background = normal;
        LicenseButton.Background = normal;

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

        RefreshMaximumGemOverview();
        Save();
    }

    private void RefreshMaximumGemOverview()
    {
        if (MaximumGemOverviewControl is null)
            return;

        MaximumGemOverviewControl.ItemsSource = null;
        MaximumGemOverviewControl.ItemsSource = ActiveCharacter.Gems;
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

    private void GemQuantityTextBox_OnChanged(object sender, TextChangedEventArgs e)
    {
        if (_loading || sender is not TextBox box || box.Tag is not GemTierEntry tier)
            return;

        tier.Quantity = ParseNonNegativeInt(box.Text);
        CascadeGemQuantityToHigherTiers(tier);
        UpdateGemTotals();
    }

    private void CascadeGemQuantityToHigherTiers(GemTierEntry changedTier)
    {
        if (CascadeGemTiersCheckBox.IsChecked != true)
            return;

        GemCollection? gem = ActiveCharacter.Gems
            .FirstOrDefault(collection => collection.Tiers.Contains(changedTier));

        if (gem is null)
            return;

        int changedIndex = gem.Tiers.IndexOf(changedTier);
        if (changedIndex < 0)
            return;

        // Nur von der gewählten höheren Stufe auf alle niedrigeren Stufen übernehmen.
        // Beispiel: Imperial Exquisit = 10 setzt alle vorherigen Stufen auf 10.
        // Trapez verändert dagegen keine höhere Stufe.
        for (int index = 0; index < changedIndex; index++)
            gem.Tiers[index].Quantity = changedTier.Quantity;
    }

    private void QuantityTextBox_OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox box)
            box.SelectAll();
    }

    private void QuantityTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox box || e.Key != Key.Enter)
            return;

        box.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        e.Handled = true;
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

    private void RefreshRunePage()
    {
        IEnumerable<RuneCollection> filtered = _activeRuneFilter switch
        {
            "Offensiv" => ActiveCharacter.Runes.Where(x => x.Category == "Offensiv"),
            "Defensiv" => ActiveCharacter.Runes.Where(x => x.Category == "Defensiv"),
            "Sonstige" => ActiveCharacter.Runes.Where(x => x.Category == "Sonstige"),
            _ => ActiveCharacter.Runes
        };

        RuneCardsControl.ItemsSource = null;
        RuneCardsControl.ItemsSource = filtered.ToList();
        UpdateRuneTotals();
        UpdateRuneFilterButtons();
    }

    private void UpdateRuneTotals()
    {
        long dust = ActiveCharacter.Runes.SelectMany(x => x.Tiers)
            .Sum(x => (long)x.DustCost * x.Quantity);
        long gold = ActiveCharacter.Runes.SelectMany(x => x.Tiers)
            .Sum(x => (long)x.GoldCost * x.Quantity);

        RuneNeededDustText.Text = dust.ToString("N0");
        RuneMissingDustText.Text =
            Math.Max(0, dust - ActiveCharacter.OwnedRuneDust).ToString("N0");
        RuneNeededGoldText.Text = gold.ToString("N0");
        RuneMissingGoldText.Text =
            Math.Max(0, gold - ActiveCharacter.OwnedRuneGold).ToString("N0");

        Save();
    }

    private void UpdateRuneFilterButtons()
    {
        Brush normal = (Brush)FindResource("Panel2Brush");
        Brush accent = (Brush)FindResource("AccentBrush");

        RuneFilterAllButton.Background = normal;
        RuneFilterOffensiveButton.Background = normal;
        RuneFilterDefensiveButton.Background = normal;
        RuneFilterOtherButton.Background = normal;

        Button active = _activeRuneFilter switch
        {
            "Offensiv" => RuneFilterOffensiveButton,
            "Defensiv" => RuneFilterDefensiveButton,
            "Sonstige" => RuneFilterOtherButton,
            _ => RuneFilterAllButton
        };

        active.Background = accent;
    }

    private void RuneResource_OnChanged(object sender, TextChangedEventArgs e)
    {
        if (_loading)
            return;

        ActiveCharacter.OwnedRuneDust =
            int.TryParse(OwnedRuneDustBox.Text, out int dust) ? Math.Max(0, dust) : 0;
        ActiveCharacter.OwnedRuneGold =
            int.TryParse(OwnedRuneGoldBox.Text, out int gold) ? Math.Max(0, gold) : 0;

        UpdateRuneTotals();
    }

    private void RuneCardPlus_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is RuneTierEntry tier)
        {
            tier.Quantity++;
            UpdateRuneTotals();
        }
    }

    private void RuneCardMinus_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is RuneTierEntry tier)
        {
            tier.Quantity = Math.Max(0, tier.Quantity - 1);
            UpdateRuneTotals();
        }
    }

    private void RuneQuantityTextBox_OnChanged(object sender, TextChangedEventArgs e)
    {
        if (_loading || sender is not TextBox box || box.Tag is not RuneTierEntry tier)
            return;

        tier.Quantity = ParseNonNegativeInt(box.Text);
        UpdateRuneTotals();
    }

    private void ClearAllRunes_OnClick(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(
                "Alle Runenmengen dieses Charakters auf 0 setzen?",
                "Bestätigung",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        foreach (RuneTierEntry tier in ActiveCharacter.Runes.SelectMany(x => x.Tiers))
            tier.Quantity = 0;

        UpdateRuneTotals();
    }

    private void SetRuneFilter(string filter)
    {
        _activeRuneFilter = filter;
        RefreshRunePage();
    }

    private void RuneFilterAll_OnClick(object sender, RoutedEventArgs e) =>
        SetRuneFilter("Alle");

    private void RuneFilterOffensive_OnClick(object sender, RoutedEventArgs e) =>
        SetRuneFilter("Offensiv");

    private void RuneFilterDefensive_OnClick(object sender, RoutedEventArgs e) =>
        SetRuneFilter("Defensiv");

    private void RuneFilterOther_OnClick(object sender, RoutedEventArgs e) =>
        SetRuneFilter("Sonstige");

    private void MortisInput_OnChanged(object sender, TextChangedEventArgs e)
    {
        if (_loading)
            return;

        MortisPlan plan = ActiveCharacter.Mortis;

        plan.TargetBones = ParseInt(MortisTargetBox.Text);
        plan.CurrentBones = ParseInt(MortisCurrentBox.Text);
        plan.AndermantPerEntry = ParseInt(MortisAndermantBox.Text);
        plan.MortisCoins = ParseInt(MortisCoinsBox.Text);
        plan.ElixirPlannedRuns = ParseInt(ElixirPlannedRunsBox.Text);
        plan.RunsPerElixir = Math.Max(1, ParseInt(RunsPerElixirBox.Text));

        UpdateMortisSummary();
    }

    private static int ParseNonNegativeInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0;

        string cleaned = value
            .Replace(".", "")
            .Replace(",", "")
            .Trim();

        return int.TryParse(cleaned, out int result)
            ? Math.Max(0, result)
            : 0;
    }

    private static int ParseInt(string value) =>
        ParseNonNegativeInt(value);

    private void MortisRunTextBox_OnChanged(object sender, TextChangedEventArgs e)
    {
        if (_loading || sender is not TextBox box || box.Tag is not MortisActivity activity)
            return;

        activity.Runs = ParseNonNegativeInt(box.Text);
        UpdateMortisSummary();
    }

    private void MortisRunPlus_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not MortisActivity activity)
            return;

        activity.Runs++;
        UpdateMortisSummary();
    }

    private void MortisRunMinus_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not MortisActivity activity)
            return;

        activity.Runs = Math.Max(0, activity.Runs - 1);
        UpdateMortisSummary();
    }

    private void UpdateMortisSummary()
    {
        if (ActiveCharacter.Mortis is null)
            return;

        MortisPlan plan = ActiveCharacter.Mortis;

        foreach (MortisActivity activity in plan.Activities)
            activity.Entries = activity.Runs;

        double progress = plan.TargetBones > 0
            ? Math.Min(100, plan.FinalBones * 100.0 / plan.TargetBones)
            : 0;

        MortisProgressBar.Value = progress;
        MortisPlannedBonesText.Text = plan.PlannedBones.ToString("N0");
        MortisMissingBonesText.Text = plan.MissingBones.ToString("N0");
        MortisAndermantCostText.Text = plan.AndermantCost.ToString("N0");
        RequiredElixirsText.Text = plan.RequiredElixirs.ToString("N0");

        MortisCompactSummaryText.Text =
            $"Runs: {plan.TotalRuns:N0}\n" +
            $"Eingänge: {plan.TotalEntries:N0}\n" +
            $"Knochen danach: {plan.FinalBones:N0}\n" +
            $"Mortis-Münzen: {plan.MortisCoins:N0}\n" +
            $"Elixiere: {plan.RequiredElixirs:N0}";

        MortisSummaryText.Text =
            $"Fortschritt: {progress:N1} %  ·  " +
            $"Ziel: {plan.TargetBones:N0}  ·  " +
            $"Aktuell: {plan.CurrentBones:N0}  ·  " +
            $"Geplant: {plan.PlannedBones:N0}  ·  " +
            $"Fehlend: {plan.MissingBones:N0}  ·  " +
            $"Überschuss: {plan.ExcessBones:N0}";

        Save();
    }

    private void ExportCharacter_OnClick(object sender, RoutedEventArgs e)
    {
        SaveFileDialog dialog = new()
        {
            Filter = "DSO Charakter (*.dsocharacter)|*.dsocharacter|JSON (*.json)|*.json",
            FileName = $"{ActiveCharacter.Name}.dsocharacter"
        };

        if (dialog.ShowDialog() != true)
            return;

        JsonSerializerOptions options = new() { WriteIndented = true };
        File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(ActiveCharacter, options));
    }

    private void ImportCharacter_OnClick(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Filter = "DSO Charakter (*.dsocharacter;*.json)|*.dsocharacter;*.json"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            string json = File.ReadAllText(dialog.FileName);
            CharacterProfile? character = JsonSerializer.Deserialize<CharacterProfile>(json);

            if (character is null)
                throw new InvalidDataException("Ungültige Charakterdatei.");

            character.Id = Guid.NewGuid();
            foreach (BuildProfile build in character.Builds)
                build.Id = Guid.NewGuid();

            AppDataService.NormalizeCharacter(character);
            _state.Characters.Add(character);
            _state.ActiveCharacterId = character.Id;
            RefreshAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Import fehlgeschlagen");
        }
    }

    private void VarnokDreamPlus_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is VarnokItem item)
        {
            item.DreamOwned++;
            RefreshVarnokRows();
            UpdateVarnokSummary();
            Save();
        }
    }

    private void VarnokDreamMinus_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is VarnokItem item)
        {
            item.DreamOwned = Math.Max(0, item.DreamOwned - 1);
            RefreshVarnokRows();
            UpdateVarnokSummary();
            Save();
        }
    }

    private void VarnokStarPlus_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is VarnokItem item)
        {
            item.StarOwned++;
            RefreshVarnokRows();
            UpdateVarnokSummary();
            Save();
        }
    }

    private void VarnokStarMinus_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is VarnokItem item)
        {
            item.StarOwned = Math.Max(0, item.StarOwned - 1);
            RefreshVarnokRows();
            UpdateVarnokSummary();
            Save();
        }
    }

    private void UpdateVarnokSummary()
    {
        VarnokPlan plan = ActiveCharacter.Varnok;

        VarnokDreamRequiredText.Text = plan.DreamRequired.ToString("N0");
        VarnokDreamMissingText.Text = plan.DreamMissing.ToString("N0");
        VarnokStarRequiredText.Text = plan.StarRequired.ToString("N0");
        VarnokStarMissingText.Text = plan.StarMissing.ToString("N0");

        VarnokProgressBar.Value = plan.Progress;

        VarnokSummaryText.Text =
            $"Fortschritt: {plan.Progress:N1} % · " +
            $"Fertig: {plan.CompletedItems} von {plan.TotalItems} Gegenständen\n" +
            $"Traumschleier vorhanden: {plan.DreamOwned:N0} / {plan.DreamRequired:N0} · " +
            $"Heilige Sterne vorhanden: {plan.StarOwned:N0} / {plan.StarRequired:N0}";

        VarnokItem? priority = plan.NextPriority;

        VarnokPriorityText.Text =
            priority is null
                ? "Alle Varnok-Ziele sind fertig."
                : $"{priority.Name}\n" +
                  $"Es fehlen {priority.DreamMissing:N0} Traumschleier " +
                  $"und {priority.StarMissing:N0} Heilige Sterne.";
    }

    private void NumericTextBox_OnGotKeyboardFocus(
        object sender,
        KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox box)
            box.SelectAll();
    }

    private void VarnokDream_OnLostFocus(
        object sender,
        KeyboardFocusChangedEventArgs e)
    {
        CommitVarnokValue(sender, isDream: true);
    }

    private void VarnokStar_OnLostFocus(
        object sender,
        KeyboardFocusChangedEventArgs e)
    {
        CommitVarnokValue(sender, isDream: false);
    }

    private void VarnokNumber_OnPreviewKeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (sender is not TextBox box)
            return;

        if (e.Key == Key.Enter)
        {
            if (box.Tag is VarnokItem item)
            {
                if (ReferenceEquals(
                    box.Parent,
                    null))
                {
                    return;
                }

                bool isDream = IsDreamTextBox(box);
                CommitVarnokValue(box, isDream);
            }

            box.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            e.Handled = true;
            return;
        }

        bool allowed =
            e.Key is >= Key.D0 and <= Key.D9 ||
            e.Key is >= Key.NumPad0 and <= Key.NumPad9 ||
            e.Key == Key.Back ||
            e.Key == Key.Delete ||
            e.Key == Key.Left ||
            e.Key == Key.Right ||
            e.Key == Key.Tab ||
            Keyboard.Modifiers.HasFlag(ModifierKeys.Control);

        if (!allowed)
            e.Handled = true;
    }

    private static bool IsDreamTextBox(TextBox box)
    {
        if (box.Parent is Grid grid)
            return Grid.GetColumn(grid) == 1;

        return true;
    }

    private void CommitVarnokValue(object sender, bool isDream)
    {
        if (_loading ||
            sender is not TextBox box ||
            box.Tag is not VarnokItem item)
        {
            return;
        }

        int value = ParseInt(box.Text);

        if (isDream)
            item.DreamOwned = value;
        else
            item.StarOwned = value;

        RefreshVarnokRows();
        UpdateVarnokSummary();
        Save();
    }

    private void RefreshVarnokRows()
    {
        _loading = true;
        VarnokCardsControl.ItemsSource = null;
        VarnokCardsControl.ItemsSource = ActiveCharacter.Varnok.Items;
        _loading = false;
    }

    private void LicenseButton_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshLicensePage();
        ShowPage(LicensePage, LicenseButton);
    }

    private void RefreshLicensePage()
    {
        LicenseService service = App.LicenseService;
        var license = service.CurrentLicense;

        LicenseStatusText.Text =
            license is null
                ? "🔴 Keine gültige Lizenz"
                : "🟢 Lizenz gültig – " + service.ExpiryText;

        LicenseIdBox.Text =
            license?.LicenseId ?? "Nicht aktiviert";

        LicenseOwnerBox.Text =
            license?.Owner ?? "–";

        LicenseExpiryBox.Text =
            service.ExpiryText;

        LicenseHardwareIdBox.Text =
            service.HardwareId;
    }

    private void CopyLicenseHardwareId_OnClick(
        object sender,
        RoutedEventArgs e)
    {
        Clipboard.SetText(App.LicenseService.HardwareId);

        MessageBox.Show(
            "Die HWID wurde kopiert.",
            "DSO Companion",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void RemoveLicense_OnClick(
        object sender,
        RoutedEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show(
            "Soll die lokale Lizenz wirklich entfernt werden?\n\n" +
            "Beim nächsten Start ist eine neue Aktivierung erforderlich.",
            "Lizenz entfernen",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        App.LicenseService.RemoveLicense();

        MessageBox.Show(
            "Die Lizenz wurde entfernt. Das Programm wird beendet.",
            "DSO Companion",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        Application.Current.Shutdown();
    }

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

    private void RunesButton_OnClick(object sender, RoutedEventArgs e)
    {
        RefreshRunePage();
        ShowPage(RunesPage, RunesButton);
    }

    private void JewelsButton_OnClick(object sender, RoutedEventArgs e) =>
        ShowPage(JewelsPage, JewelsButton);

    private void MortisButton_OnClick(object sender, RoutedEventArgs e)
    {
        UpdateMortisSummary();
        ShowPage(MortisPage, MortisButton);
    }

    private void VarnokButton_OnClick(object sender, RoutedEventArgs e)
    {
        ActiveCharacter.Varnok ??= AppDataService.CreateVarnokPlan();

        if (ActiveCharacter.Varnok.Items is null ||
            ActiveCharacter.Varnok.Items.Count == 0)
        {
            ActiveCharacter.Varnok = AppDataService.CreateVarnokPlan();
        }

        ShowPage(VarnokPage, VarnokButton);
        RefreshVarnokRows();
        UpdateVarnokSummary();
    }

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
