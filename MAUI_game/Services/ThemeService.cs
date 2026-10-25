using MAUI_game.Resources.Localization;

namespace MAUI_game.Services
{
    public static class ThemeService
    {
        public static event Action? ThemeChanged;

        public static void Initialize()
        {
            string savedTheme = Preferences.Get("AppTheme", "Dark");
            ApplyTheme(savedTheme);

            LanguageService.LanguageChanged += () => ThemeChanged?.Invoke();
        }

        public static void SetTheme(string themeName)
        {
            Preferences.Set("AppTheme", themeName);
            ApplyTheme(themeName);
        }

        private static void ApplyTheme (string themeName)
        {
            if (Application.Current == null) return;

            switch (themeName)
            {
                case "Light":
                    Application.Current.UserAppTheme = AppTheme.Light;
                    break;
                case "Dark":
                    Application.Current.UserAppTheme = AppTheme.Dark;
                    break;
                default:
                    Application.Current.UserAppTheme = AppTheme.Unspecified;
                    break;
            }
            // Teavitame kuulajaid
            ThemeChanged?.Invoke();
        }

        public static string GetCurrentTheme()
        {
            return Preferences.Get("AppTheme", "Dark");
        }

        // Abimeetod, mis aitab Pickerist valitud tõlgitud teksti muuta tagasi "Dark" või "Light" koodiks
        public static string GetThemeCode(string localizedName)
        {
            if (localizedName == AppResources.LightTheme)
                return "Light";

            return "Dark"; // Vaikimisi Dark
        }
    }
}
