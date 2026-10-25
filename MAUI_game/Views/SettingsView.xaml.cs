using MAUI_game.ViewModels;
using MAUI_game.Services;

namespace MAUI_game.Views;

public partial class SettingsView : ContentView
{
	public event EventHandler CloseRequested;

    public SettingsView()
	{
		InitializeComponent();
        BindingContext = new GameLanguageViewModel();

        LanguageService.LanguageChanged += OnLanguageChanged;
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

    private void OnCloseClicked(object sender, EventArgs e)
	{
		CloseRequested?.Invoke(this, EventArgs.Empty);
	}
}