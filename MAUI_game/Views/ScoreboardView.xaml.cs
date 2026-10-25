using MAUI_game.Models;
using MAUI_game.Resources.Localization;
using MAUI_game.Services;

namespace MAUI_game.Views;

public partial class ScoreboardView : ContentView
{
    private readonly DatabaseService _databaseService = new DatabaseService();

    // Sündmus, et anda pealehele (MainPage) teada, kui kasutaja soovib edetabeli sulgeda
    public event EventHandler CloseRequested;

    public ScoreboardView()
    {
        InitializeComponent();
    }

    // Andmete laadimine andmebaasist
    public async Task LoadScoresAsync()
    {
        var scores = await _databaseService.GetScoresAsync();
        ScoreCollectionView.ItemsSource = scores;
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    // Kirje kustutamine andmebaasist ja vaate värskendamine
    private async void OnDeleteScoreClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ScoreModel score)
        {
            await _databaseService.DeleteScoreAsync(score);
            // Laeme edetabeli uuesti, et eemaldatud kirje kaoks
            await LoadScoresAsync();
        }
    }

    private async void OnEditScoreClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is ScoreModel scoreToEdit)
        {
            // Küsime kasutajalt uut nime (vaikimisi väärtuseks on vana nimi)
            string newName = await Application.Current?.MainPage?.DisplayPromptAsync(
                AppResources.ChangeName,
                AppResources.ChangeNameInsert,
                initialValue: scoreToEdit.PlayerName,
                maxLength: 15);

            // Kui kasutaja vajutas OK ja sisestas midagi (mitte tühi string)
            if (!string.IsNullOrWhiteSpace(newName))
            {
                // Uuendame mudeli nime
                scoreToEdit.PlayerName = newName.Trim();

                // Salvestame muudatuse andmebaasi (UpdateAsync)
                await _databaseService.UpdateScoreAsync(scoreToEdit);

                // Laeme edetabeli uuesti, et vaade värskenduks
                await LoadScoresAsync();
            }
        }
    }
}