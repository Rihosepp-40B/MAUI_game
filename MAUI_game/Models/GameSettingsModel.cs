namespace MAUI_game.Models
{
    public static class GameSettingsModel
    {
        private const string SoundSettingKey = "IsSoundEnabled";

        public static bool IsSoundEnabled
        {
            get => Preferences.Get(SoundSettingKey, true);
            set => Preferences.Set(SoundSettingKey, value);
        }
    }
}
