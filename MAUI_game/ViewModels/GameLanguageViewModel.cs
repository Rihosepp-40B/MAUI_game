using System.ComponentModel;
using System.Runtime.CompilerServices;
using MAUI_game.Services;
using MAUI_game.Resources.Localization;
using MAUI_game.Models;

namespace MAUI_game.ViewModels
{
    public class GameLanguageViewModel : INotifyPropertyChanged
    {
        private Dictionary<string, TowerTypeModel> _towerCatalog;
        public Dictionary<string, TowerTypeModel> TowerCatalog
        {
            get => _towerCatalog;
            set
            {
                if (_towerCatalog != value)
                {
                    _towerCatalog = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<string> ThemeList => new()
        {
            AppResources.DarkTheme,
            AppResources.LightTheme
        };

        public Dictionary<string, string> LanguageMap { get; } = new()
        {
            { AppResources.English, "en" },
            { AppResources.Estonia, "et" }
        };
        public List<string> Languages => LanguageMap.Keys.ToList();

        private string _selectedLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    OnPropertyChanged();

                    if (_selectedLanguage != null && LanguageMap.TryGetValue(_selectedLanguage, out var code))
                    {
                        ChangeLanguage(code);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Lives => AppResources.Lives;
        public string Back => AppResources.Back;
        public string ChooseHere => AppResources.ChooseHere;
        public string ChooseLanguage => AppResources.ChooseLanguage;
        public string ChooseTheme => AppResources.ChooseTheme;
        public string EarnedScore => AppResources.EarnedScore;
        public string FinalScore => AppResources.FinalScore;
        public string GameOver => AppResources.GameOver;
        public string Gold => AppResources.Gold;
        public string Highscore => AppResources.Highscore;
        public string InsertName => AppResources.InsertName;
        public string MG => AppResources.MG;
        public string Mortar => AppResources.Mortar;
        public string NextWave => AppResources.NextWave;
        public string OutOfLives => AppResources.OutOfLives;
        public string Pause => AppResources.Pause;
        public string Paused => AppResources.Paused;
        public string Resume => AppResources.Resume;
        public string SaveResult => AppResources.SaveResult;
        public string Score => AppResources.Score;
        public string Settings => AppResources.Settings;
        public string UnusedGold => AppResources.UnusedGold;
        public string WaveLvl => AppResources.WaveLvl;
        public string Reset => AppResources.Reset;
        public string HP => AppResources.HP;

        public GameLanguageViewModel()
        {
            TowerCatalog = TowerRepository.GetCatalog();

            LanguageService.LanguageChanged += OnLanguageChanged;
        }

        private void ChangeLanguage(string code)
        {
            LanguageService.ChangeLanguage(code);

            string foundKey = LanguageMap.FirstOrDefault(x => x.Value == code).Key;
            Preferences.Set("SelectedLanguage", foundKey);
        }

        private void OnLanguageChanged()
        {
            TowerCatalog = TowerRepository.GetCatalog();

            OnPropertyChanged(nameof(Lives));
            OnPropertyChanged(nameof(Gold));
            OnPropertyChanged(nameof(Score));
            OnPropertyChanged(nameof(WaveLvl));
            OnPropertyChanged(nameof(NextWave));
            OnPropertyChanged(nameof(Pause));
            OnPropertyChanged(nameof(Resume));
            OnPropertyChanged(nameof(Reset));
            OnPropertyChanged(nameof(Highscore));
            OnPropertyChanged(nameof(Highscore));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                
    }
}
