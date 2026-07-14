using System.Windows;
using System.Windows.Controls;
using DSOCompanion.Models;
using DSOCompanion.Services;

namespace DSOCompanion;

public partial class MainWindow : Window
{
    private readonly StorageService _storage = new();
    private AppState _state;
    private bool _loading;

    private CharacterProfile ActiveCharacter =>
        _state.Characters.First(x => x.Id == _state.ActiveCharacterId);

    public MainWindow()
    {
        InitializeComponent();

        ClassCombo.ItemsSource = AppDataService.Classes;
        ServerCombo.ItemsSource = AppDataService.Servers;

        _state = _storage.Load();
        NormalizeState();
        RefreshAll();
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
        {
            if (character.Builds.Count == 0)
            {
                BuildProfile build = new() { Name = "Boss-Build" };
                character.Builds.Add(build);
                character.ActiveBuildId = build.Id;
            }

            if (character.ActiveBuildId is null ||
                character.Builds.All(x => x.Id != character.ActiveBuildId))
            {
                character.ActiveBuildId = character.Builds[0].Id;
            }
        }
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

        BuildList.ItemsSource = null;
        BuildList.ItemsSource = ActiveCharacter.Builds;
        BuildList.SelectedItem = ActiveCharacter.Builds
            .FirstOrDefault(x => x.Id == ActiveCharacter.ActiveBuildId);

        _loading = false;

        UpdateSummary();
        Save();
    }

    private void UpdateSummary()
    {
        SummaryCharacterText.Text = ActiveCharacter.Name;
        SummaryClassText.Text =
            $"{ActiveCharacter.CharacterClass} · {ActiveCharacter.Server} · Level {ActiveCharacter.Level}";
        SummaryBuildCountText.Text = ActiveCharacter.Builds.Count.ToString();
    }

    private void Save()
    {
        _storage.Save(_state);
    }

    private static string? Prompt(string title, string initialValue)
    {
        Window dialog = new()
        {
            Title = title,
            Width = 420,
            Height = 170,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = System.Windows.Media.Brushes.Black,
            Foreground = System.Windows.Media.Brushes.White
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

    private void CharacterCombo_OnSelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
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

        MessageBoxResult result = MessageBox.Show(
            "Diesen Charakter wirklich löschen?",
            "Bestätigung",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

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

        UpdateSummary();
        Save();

        CharacterCombo.Items.Refresh();
    }

    private void BuildList_OnSelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_loading || BuildList.SelectedItem is not BuildProfile build)
            return;

        ActiveCharacter.ActiveBuildId = build.Id;
        Save();
    }

    private void AddBuild_OnClick(object sender, RoutedEventArgs e)
    {
        string? name = Prompt("Neuen Build anlegen", "Neuer Build");

        if (string.IsNullOrWhiteSpace(name))
            return;

        BuildProfile build = new() { Name = name };
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

        BuildProfile copy = new() { Name = name };
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

        MessageBoxResult result = MessageBox.Show(
            "Diesen Build wirklich löschen?",
            "Bestätigung",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        ActiveCharacter.Builds.Remove(build);
        ActiveCharacter.ActiveBuildId = ActiveCharacter.Builds[0].Id;
        RefreshAll();
    }
}
