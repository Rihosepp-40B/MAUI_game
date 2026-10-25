using MAUI_game.Models;
using MAUI_game.Resources.Localization;
using MAUI_game.Services;
using MAUI_game.ViewModels;
using System.Diagnostics;

namespace MAUI_game.Views
{
    public partial class MainPage : ContentPage, IDisposable
    {
        private readonly DatabaseService _databaseService = new DatabaseService();
        private double _cachedCutoutWidthDp = 0; // Siia salvestame kaamera riba mõõdetud laiuse
        private bool isGamePaused = false;

        private Stopwatch _stopwatch = new Stopwatch();
        private TimeSpan _lastTime;
        private IDispatcherTimer gameTimer;
        private GameUpdateServices _gameUpdater;

        private double _uiUpdateTimer = 0;

        public GameLanguageViewModel LanguageVM { get; } = new();
        public Dictionary<string, TowerTypeModel> TowerCatalog { get; set; } = new();

        public List<EnemyModel> Enemies => GameWorldInstance.Enemies;
        public List<TowerModel> Towers => GameWorldInstance.Towers;
        public List<MapPointModel> PathPoints => GameWorldInstance.PathPoints;
        private readonly DrawGameService _drawGameService;

        // Konstruktor
        public MainPage(DrawGameService drawGameService)
        {
            LoadTowerCatalog();
            InitializeComponent();
            
            BindingContext = this;
            _gameUpdater = new GameUpdateServices(
                Enemies, 
                Towers, 
                PathPoints, 
                GameWorldInstance.CurrentPlayer, 
                GameWorldInstance.ActiveDirectfireEffects, 
                GameWorldInstance.ActiveExplosions, 
                GameWorldInstance.ActiveProjectiles
                );

            _gameUpdater._enemyManager.OnGameOver += HandleGameOver;
            InitializeGameLoop();
            
            DeviceDisplay.MainDisplayInfoChanged += OnDeviceDisplayInfoChanged;

            GameSettingsView.CloseRequested += (s, e) =>
            {
                LoadTowerCatalog();

                GameStopScreen();

                BindingContext = null;
                BindingContext = this;

                GameSettingsView.IsVisible = false;
                PausMenuLayout.IsVisible = true;
            };

            ScoreboardView.CloseRequested += (s, e) =>
            {
                GameStopScreen();
                ScoreboardView.IsVisible = false;
                PausMenuLayout.IsVisible = true;
            };
            _drawGameService = drawGameService;
            ThemeService.ThemeChanged -= OnThemeChanged; // Eemaldame igaks juhuks vana, kui see on juba olemas
            ThemeService.ThemeChanged += OnThemeChanged; // Lisame uue
        }

        private void OnThemeChanged()
        {
            // Kui teema muutub, anname lehel olevale GraphicsView-le käsu pilt uuesti joonistada
            GameGraphicsView.Invalidate();
        }

        public void LoadTowerCatalog()
        {
            TowerCatalog = TowerRepository.GetCatalog();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            ThemeService.ThemeChanged -= OnThemeChanged;
            ThemeService.ThemeChanged += OnThemeChanged;

            // Mõõdame ja salvestame laiuse
            _cachedCutoutWidthDp = await DisplayCutoutService.CheckAndMeasureCutout();

            // Rakendame paddingu
            ApplyPaddingBasedOnRotation(DeviceDisplay.Current.MainDisplayInfo.Rotation);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DeviceDisplay.MainDisplayInfoChanged -= OnDeviceDisplayInfoChanged;
            ThemeService.ThemeChanged -= OnThemeChanged;
        }

        private void OnDeviceDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
        {
            ApplyPaddingBasedOnRotation(e.DisplayInfo.Rotation);
        }

        private void ApplyPaddingBasedOnRotation(DisplayRotation rotation)
        // rakendame kaamera riba vastaskülje kuva
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Küsime abiklassilt arvutatud Thickness väärtuse
                RootGrid.Padding = DisplayCutoutService.GetPaddingForRotation(rotation, _cachedCutoutWidthDp);
            });
        }

        private string selectedTowerType = null;

        private void UpdateTowerButtonStyles()
        {
            // Käime läbi kõik nupud, mis asuvad meie nupupaneelis
            foreach (var child in TowerButtonsLayout.Children)
            {
                if (child is Button btn && btn.CommandParameter is string towerId)
                {
                    btn.BorderColor = (towerId == selectedTowerType) ? Colors.Red : Colors.LightGray;
                }
            }
        }

        private DateTime lastClickTime = DateTime.MinValue;
        private string lastClickedTowerTypeId = string.Empty;

        private void OnTowerCatalogItemClicked(object sender, EventArgs e)
        {
            // Võtame nupu küljest torni ID (eeldusel)
            if (sender is Button button && button.CommandParameter is string towerId)
            {
                DateTime now = DateTime.Now;
                double timeSinceLastClick = (now - lastClickTime).TotalMilliseconds;

                // Kui klõpsati samale nupule ja aeg on alla 300 ms -> see on TOPELTKLÕPS!
                if (towerId == lastClickedTowerTypeId && timeSinceLastClick < 300)
                {
                    OpenTowerPopup(towerId);

                    // Nullime aja, et kolmas klõps ei käivitaks uuesti
                    lastClickTime = DateTime.MinValue;
                    lastClickedTowerTypeId = string.Empty;
                }
                else
                {
                    // Ühekordne klõps
                    lastClickTime = now;
                    lastClickedTowerTypeId = towerId;

                    // Valime torni ehitamiseks
                    SelectTowerForBuilding(towerId);
                }
            }
        }

        // Torni info popup avamine
        private async void OpenTowerPopup(string towerId)
        {
            if (TowerCatalog.TryGetValue(towerId, out var towerInfo))
            {                
                await DisplayAlert(towerInfo.Name, 
                    $"{AppResources.Damage} {towerInfo.Damage}\n" +
                    $"{AppResources.FireType} {(towerInfo.IsDirectFire ? AppResources.DirectFire : AppResources.IndirectFire)}\n" +
                    $"{AppResources.DamageArea} {towerInfo.DamageArea}{(towerInfo.SplashDamageArea > 0 ? $" / {towerInfo.SplashDamageArea}" : "")}\n" +
                    $"{AppResources.Range} {towerInfo.Range}\n" +
                    $"{AppResources.Range} {towerInfo.Firerate}rpm\n" +
                    $"{AppResources.TravelTime} {towerInfo.TravelTime}s\n" +
                    $"{AppResources.Size} {towerInfo.SizeRadius}\n" +
                    $"{AppResources.HP} {towerInfo.Health}\n\n" +
                    $"{AppResources.Cost} {towerInfo.Cost}g\n\n" +
                    $"{AppResources.DragInstructions}", 
                    "OK");
            }
        }

        // Torni valimine ehitamiseks
        private async void SelectTowerForBuilding(string towerId)
        {
            if (isGamePaused) return;
            selectedTowerType = towerId;
            UpdateTowerButtonStyles();
        }

        //Vajutus mängualale (alustab lohistamist)
        private void OnCanvasStartInteraction(object sender, TouchEventArgs e)
        {
            if (isGamePaused || string.IsNullOrEmpty(selectedTowerType) || e.Touches.Length == 0) return;

            var touch = e.Touches[0];

            // Otsime TowerCatalog-ist valitud torni andmed
            if (TowerCatalog.TryGetValue(selectedTowerType, out var towerInfo))
            {
                GameWorldInstance.IsDragging = true;
                GameWorldInstance.DraggingRange = towerInfo.Range; // Võtab õige ulatuse
                GameWorldInstance.DraggingColor = towerInfo.Color; // Võtab õige värvi
                GameWorldInstance.DraggingSizeRadius = towerInfo.SizeRadius; // Võtab õige suuruse
                GameWorldInstance.CurrentDragPos = new PointF((float)touch.X, (float)touch.Y);
                GameGraphicsView.Invalidate();
            }
        }

        //Lohistamine (sõrme/hiire liigutamine ekraanil)
        private void OnCanvasDragInteraction(object sender, TouchEventArgs e)
        {
            if (isGamePaused || !GameWorldInstance.IsDragging || e.Touches.Length == 0) return;

            var touch = e.Touches[0];
            GameWorldInstance.CurrentDragPos = new PointF((float)touch.X, (float)touch.Y);
            GameGraphicsView.Invalidate(); // Värskendab kaarti ja liigutab raadiust kaasa
        }

        //Lahtilaskmine (ehitab torni)
        private void OnCanvasEndInteraction(object sender, TouchEventArgs e)
        {
            // KUI MÄNG ON PAUSIL, EI TEE MIDAGI!
            if (isGamePaused || !GameWorldInstance.IsDragging) return;

            // Viimane teadaolev positsioon lahtilaskmisel
            float dropX = GameWorldInstance.CurrentDragPos.X;
            float dropY = GameWorldInstance.CurrentDragPos.Y;

            // Lõpetame lohistamise eelvaate
            GameWorldInstance.IsDragging = false;
            GameGraphicsView.Invalidate();

            if (!string.IsNullOrEmpty(selectedTowerType) && TowerCatalog.TryGetValue(selectedTowerType, out var towerInfo))
            {
                // Kontroll, kas raha on piisavalt
                if (GameWorldInstance.CurrentPlayer.Gold < towerInfo.Cost) return;

                // Kontrollime kohta arvestades torni tegelikku suurust
                if (BuildService.CanBuildAt(dropX, dropY, towerInfo.SizeRadius, PathPoints, Towers))
                {
                    // Raha mahavõtmine
                    GameWorldInstance.CurrentPlayer.Gold -= towerInfo.Cost;

                    // Arvutame Firerate (lasku minutis) ümber kaadriteks (30 FPS * 60 sek = 1800 kaadrit minutis)
                    int attackFrames = towerInfo.Firerate > 0 ? 1800 / towerInfo.Firerate : 30;

                    int travelFrames = (int)(towerInfo.TravelTime * 30);

                    var newTower = new TowerModel
                    {
                        X = dropX,
                        Y = dropY,
                        TowerTypeId = towerInfo.Id,
                        //TowerTypeName = towerInfo.Name,
                        Cost = towerInfo.Cost,
                        Damage = towerInfo.Damage,
                        DamageArea = towerInfo.DamageArea,
                        SplashDamageArea = towerInfo.SplashDamageArea,
                        Range = towerInfo.Range,
                        TravelTime = towerInfo.TravelTime,
                        MaxHealth = towerInfo.Health,
                        Firerate = towerInfo.Firerate,
                        TowerColor = towerInfo.Color,
                        SizeRadius = towerInfo.SizeRadius, // Salvestame ehitatud torni suuruse.
                        IsDirectFire = towerInfo.IsDirectFire,

                        CooldownTimer = 0,
                        AttackSpeedFrames = attackFrames, // Laskude vahel olevate kaadrite arv.
                        AttackTravelFrames = travelFrames,
                        CurrentHealth = towerInfo.Health,
                    };

                    GameWorldInstance.Towers.Add(newTower);
                    GameGraphicsView.Invalidate();
                }
            }
        }

        // Käivitab mängu loopi
        private void InitializeGameLoop()
        {
            _stopwatch.Start();
            _lastTime = _stopwatch.Elapsed;

            gameTimer = Dispatcher.CreateTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(30); // ~33 FPS
            gameTimer.Tick += (s, e) =>
            {
                //Arvutame täpse aja (deltaTime sekundites)
                TimeSpan currentTime = _stopwatch.Elapsed;
                double deltaTime = (currentTime - _lastTime).TotalSeconds;
                _lastTime = currentTime;

                // Kaitseks, kui akna fookus kaob ja taimer teeb suure hüppe
                if (deltaTime > 0.1) deltaTime = 0.033;

                //Uuenda mängu
                _gameUpdater.UpdateGame(deltaTime);

                //Värskenda graafikat
                GameGraphicsView.Invalidate();

                // Uus updateUI, teeb harvem, et säästa jõudlust
                _uiUpdateTimer += deltaTime;
                if (_uiUpdateTimer >= 0.15)
                {
                    UpdateUI();
                    _uiUpdateTimer = 0;
                }
            };
            gameTimer.Start();
        }

        //Värskendab XAML välju
        private void UpdateUI()
        {
            if (GameWorldInstance.CurrentPlayer != null)
            {
                LblLives.Text = GameWorldInstance.CurrentPlayer.CurrentHealth.ToString();
                LblGold.Text = GameWorldInstance.CurrentPlayer.Gold.ToString();
                LblScore.Text = GameWorldInstance.CurrentPlayer.Score.ToString();
            }

            var waveMgr = _gameUpdater?._waveManager;
            if (waveMgr != null)
            {
                if (waveMgr.waveTimer == 0)
                { LblWaveTimer.Text = "0"; }
                else
                {
                    int waveTimer = (int)(waveMgr.timeBetweenWaves + 1 - waveMgr.waveTimer);
                    LblWaveTimer.Text = waveTimer.ToString();
                }
                LblWaveLevel.Text = _gameUpdater._waveManager.currentWave.ToString();
            }
            
        }

        //Pausi nupule vajutamine
        private void OnPauseClicked(object sender, EventArgs e)
        {
            PauseGame();
        }

        //Pausi sisse ja välja lülitamine
        private void PauseGame()
        {
            isGamePaused = !isGamePaused;
            GameStopScreen();

            if (isGamePaused)
            {
                gameTimer?.Stop();
                BtnPause.IsVisible = false;
            }
            else
            {
                // Käivitame taimeri uuesti tööle
                gameTimer?.Start();
                BtnPause.Text = $"{AppResources.Pause}";
                BtnPause.IsVisible = true;

                GameSettingsView.IsVisible = false;
                PausMenuLayout.IsVisible = true;
            }
        }

        //Hall varjund mängule, et osutada, et mäng on pausil
        private void GameStopScreen()
        {
            GrdGamePauseOver.IsVisible = isGamePaused;
            int savedHighScore = Preferences.Get("HighScore", 0);
            string savedHighScoreName = Preferences.Get("HighScoreName", "NoGame");
            LblHighScore.Text = $"{AppResources.Highscore} {savedHighScoreName} {savedHighScore}";
        }

        //GameOVer olukorra kuvamine
        private async void HandleGameOver(object sender, EventArgs e)
        {
            //Peatame taimeri / mängu
            isGamePaused = true;
            gameTimer?.Stop(); // Või sinu taimeri muutuja

            BtnPause.IsVisible = false;
            BtnResume.IsVisible = false;
            BtnReset.IsVisible = true;

            LblGamePaused.IsVisible = false;
            LblGameOver.IsVisible = true;
            GameStopScreen();

            //Lõpuaken
            EndGold.Text = $"{AppResources.UnusedGold} {GameWorldInstance.CurrentPlayer.Gold}g";
            EndScore.Text = $"{AppResources.EarnedScore} {GameWorldInstance.CurrentPlayer.Score}";
            TotalScore.Text = $"{AppResources.FinalScore} {GameWorldInstance.CurrentPlayer.TotalScore}";
            PlayerNameEntry.Placeholder = $"{GameWorldInstance.CurrentPlayer.Name}";
            EndPopup.IsVisible = true;
        }

        //Lõpuakna nime esitamise (salvestamise) nupule vajutamine
        private async void OnSubmitNameClicked(object sender, EventArgs e)
        {
            string playerName = PlayerNameEntry.Text;

            string finalName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();
            GameWorldInstance.CurrentPlayer.Name = finalName;

            int finalScore = GameWorldInstance.CurrentPlayer.TotalScore;

            // Salvestame SQLite andmebaasi
            var newScoreRecord = new ScoreModel
            {
                PlayerName = finalName,
                Score = finalScore
            };
            await _databaseService.SaveScoreAsync(newScoreRecord);

            CheckAndSaveHighScore(finalScore, finalName);

            EndPopup.IsVisible = false;
        }

        //Reset nupule vajautamine
        private void OnResetClicked(object sender, EventArgs e) => ResetGame();

        //Tegevused kui käivitakse reset
        private void ResetGame()
        {
            BtnPause.IsVisible = true;
            BtnResume.IsVisible = true;
            BtnReset.IsVisible = false;

            LblGamePaused.IsVisible = true;
            LblGameOver.IsVisible = false;

            GameWorldInstance.ResetDrawable();
            _gameUpdater._waveManager.ResetWave();

            UpdateUI();
            isGamePaused = false;
            GameStopScreen();
            gameTimer?.Start();
        }

        //Vaatab olemas olevat kõrgimat skoori ja võrdleb praegust skoori
        public void CheckAndSaveHighScore(int currentTotalScore, string currentPlayer)
        {
            // Loeme senise rekordi (vaikimisi on 0, kui seda pole varem salvestatud)
            int savedHighScore = Preferences.Get("HighScore", 0);
            string savedHighScoreName = Preferences.Get("HighScoreName", "NoGame");

            if (currentTotalScore > savedHighScore)
            {
                // Salvestame uue rekordi seadme mällu
                Preferences.Set("HighScore", currentTotalScore);
                Preferences.Set("HighScoreName", currentPlayer);
            }
            GameStopScreen();
        }

        private void OnOpenSettingsClicked(object sender, EventArgs e)
        {
            PausMenuLayout.IsVisible = false;      // Peidab tavalise pausimenüü
            GameSettingsView.IsVisible = true;       // Kuvab teise XAML-i sisu
        }

        private async void OnOpenScoreboardClicked(object sender, EventArgs e)
        {
            PausMenuLayout.IsVisible = false;      // Peidab tavalise pausimenüü
            ScoreboardView.IsVisible = true;       // Kuvab edetabeli vaate
            await ScoreboardView.LoadScoresAsync(); // Laeb värsked andmed andmebaasist
        }

        public void Dispose()
        {
            ThemeService.ThemeChanged -= OnThemeChanged;
        }
    }
}