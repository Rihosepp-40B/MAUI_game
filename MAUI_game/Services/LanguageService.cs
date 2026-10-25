using MAUI_game.Resources.Localization;
using MAUI_game.ViewModels;
using System.Globalization;

namespace MAUI_game.Services
{
    public static class LanguageService
    {
        public static event Action? LanguageChanged;

        //Käivitatakse äpi alguses, et taastada salvestatud keel
        public static void Initialize()
        {
            // Otsime mälust salvestatud keelt, vaikimisi on "en" (või "et")
            string savedLanguage = Preferences.Get("AppLanguage", "en");
            ChangeLanguage(savedLanguage);
        }

        public static void ChangeLanguage(string languageCode)
        {
            var culture = new CultureInfo(languageCode);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            AppResources.Culture = culture;

            Preferences.Set("AppLanguage", languageCode);
            LanguageChanged?.Invoke();
        }

        public static string GetCurrentLanguage()
        {
            return Preferences.Get("SelectedLanguage", "English");
        }
    }
}
