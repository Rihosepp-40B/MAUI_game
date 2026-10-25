using MAUI_game.ViewModels;
using MAUI_game.Services;
using MAUI_game.Models;

namespace MAUI_game.Views;

public partial class SettingsView : ContentView
{
	public event EventHandler CloseRequested;

    public SettingsView()
	{
		InitializeComponent();
        BindingContext = new GameLanguageViewModel();

        LanguageService.LanguageChanged += OnLanguageChanged;

        SoundSwitch.IsToggled = GameSettingsModel.IsSoundEnabled;
    }

    // Värskendab settings menüü keelt
    private void OnLanguageChanged()
    {
        // Kasutame Dispatcher-it, et MAUI uuendaks UI-d turvaliselt pealõimel
        Dispatcher.Dispatch(() =>
        {
            // Määrame uue mudeli, mis tõmbab uued tõlked ressurssidest
            BindingContext = new GameLanguageViewModel();
        });
    }

    private void OnThemePickerChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker && picker.SelectedItem is string selectedTheme)
        {
            string themeCode = ThemeService.GetThemeCode(selectedTheme);
            ThemeService.SetTheme(themeCode);
        }
    }

    private void OnSoundSwitchToggled(object sender, ToggledEventArgs e)
    {
        // Salvestame uue väärtuse globaalsesse klassi
        GameSettingsModel.IsSoundEnabled = e.Value;
    }

    private void OnCloseClicked(object sender, EventArgs e)
	{
		CloseRequested?.Invoke(this, EventArgs.Empty);
	}
}