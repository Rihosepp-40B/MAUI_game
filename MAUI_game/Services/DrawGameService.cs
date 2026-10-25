using MAUI_game.Models;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace MAUI_game.Services
{
    public class DrawGameService : IDrawable
    {
        private Color _currentFillColor;
        private Color _currentStrokeColor;
        //private readonly GraphicsView? _graphicsView; // kas on vaja? VANA
        private IImage enemyImage;

        private GameUpdateServices _gameUpdater;

        private static readonly Color ValidBuildColor = Color.FromArgb("#3300FF00");
        private static readonly Color InvalidBuildColor = Color.FromArgb("#33FF0000");
        private PathF _cachedRoadPath = null;

        // Konstruktor
        public DrawGameService()
        {

            // Laadime pildi mällu juba objekti loomisel
            LoadImages();
            UpdateColors();

            _gameUpdater = new GameUpdateServices(
                Enemies, 
                Towers, 
                PathPoints, 
                CurrentPlayer, 
                ActiveDirectfireEffects, 
                ActiveExplosions, 
                ActiveProjectiles
                );
            ThemeService.ThemeChanged -= OnThemeChanged; // Eemaldame igaks juhuks vana, kui see on juba olemas
            ThemeService.ThemeChanged += OnThemeChanged; // Lisame uue
        }

        private async void LoadImages()
        {
            try
            {
                // Avab pildi Resources/Images kaustast
                using var stream = await FileSystem.OpenAppPackageFileAsync("enemy.png");
                enemyImage = PlatformImage.FromStream(stream);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Pildi laadimise viga: {ex.GetType().Name} - {ex.Message}");
            }
        }

        public PlayerModel CurrentPlayer { get; set; } = new PlayerModel();

        // Ehitatamisel ja lohistamisel torni muutujad
        public bool IsDragging { get; set; } = false;
        public PointF CurrentDragPos { get; set; }
        public int DraggingRange { get; set; }  // Dünaamiline ulatus
        public Color DraggingColor { get; set; } // Dünaamiline värv
        public int DraggingSizeRadius { get; set; }  // DÜnaamiline suurus

        // Aktiivsed lasud
        public List<ProjectileModel> ActiveProjectiles { get; set; } = new();
        public List<DirectfireEffectModel> ActiveDirectfireEffects { get; set; } = new();
        public List<ExplosionEffectModel> ActiveExplosions{ get; set; } = new();

        public List<MapPointModel> PathPoints { get; set; } = new();

        public List<TowerModel> Towers { get; set; } = new();
        public List<EnemyModel> Enemies { get; set; } = new();

        //Juhuslikute andmetega raja loomine
        public void GenerateRandomPath(float canvasWidth, float canvasHeight)
        {
            Random rand = new Random();
            PathPoints.Clear();

            // Alustame vasakult servast, näiteks keskkohast vertikaalselt
            float currentX = 0;
            float currentY = (float)rand.Next(100, (int)(canvasHeight - 100));

            PathPoints.Add(new MapPointModel(currentX, currentY));

            // Määrame, mitu punkti (pööret) rajal on
            int pointCount = 6;
            float stepSize = canvasWidth / pointCount; // Jaotame ekraani laiali

            for (int i = 1; i < pointCount; i++)
            {
                // Liigume alati natuke paremale
                currentX += stepSize;

                // Valime uue Y-koordinaadi juhuslikult, aga hoiame mänguala piirides
                // (näiteks jätame 50 pikslit varu ülaservast ja alaservast)
                currentY = (float)rand.Next(50, (int)(canvasHeight - 50));

                PathPoints.Add(new MapPointModel(currentX, currentY));
            }

            // Viimane punkt võib minna otse välja paremast servast
            PathPoints.Add(new MapPointModel(canvasWidth, currentY));

            RebuildRoadPath();
        }

        private void RebuildRoadPath()
        {
            if (PathPoints.Count == 0) return;

            _cachedRoadPath = new PathF();
            _cachedRoadPath.MoveTo(PathPoints[0].X, PathPoints[0].Y);
            for (int i = 1; i < PathPoints.Count; i++)
            {
                _cachedRoadPath.LineTo(PathPoints[i].X, PathPoints[i].Y);
            }
        }

        public bool isPathGenerated = false;

        private void OnThemeChanged()
        {
            // Uuendame värvid
            UpdateColors();
        }

        public void UpdateColors()
        {
            bool isDarkMode = Application.Current?.UserAppTheme == AppTheme.Dark;
            _currentFillColor = isDarkMode ? Colors.DarkSlateGray : Colors.DarkSeaGreen;
            _currentStrokeColor = isDarkMode ? Color.FromHex("#111111") : Colors.DimGrey;
        }

        public void Dispose()
        {
            ThemeService.ThemeChanged -= OnThemeChanged;
        }

        //Kogu mängu objektide joonistamine
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // Salvestame automaatselt jooksva ekraani mõõdud
            CanvaSizeModel.CanvasWidth = dirtyRect.Width;
            CanvaSizeModel.CanvasHeight = dirtyRect.Height;
            var CanvasWidth = CanvaSizeModel.CanvasWidth;
            var CanvasHeight = CanvaSizeModel.CanvasHeight;

            // Genereerime juhusliku raja täpselt üks kord, kui ekraani mõõdud on olemas
            if (!isPathGenerated && CanvasWidth > 0 && CanvasHeight > 0)
            {
                GenerateRandomPath(CanvasWidth, CanvasHeight);
                isPathGenerated = true;
            }

            //Mänguala taust (terve ala on ehitatav)
            canvas.FillColor = _currentFillColor;
            canvas.FillRectangle(dirtyRect);

            DrawRoad(canvas);
            DrawTowers(canvas);
            DrawEnemy(canvas);
            DrawExplosion(canvas);
            DrawDirectfireBeams(canvas);
            DrawProjectiles(canvas);
            DragPreview(canvas);
        }

        //Plahvatuse joonistamine
        private void DrawExplosion(ICanvas canvas)
        {
            foreach (var expl in ActiveExplosions)
            {
                // 1. Arvutame edenemise 0.0-st (algus) 1.0-ni (lõpp)
                float progress = 1.0f - ((float)expl.FramesRemaining / expl.TotalFrames);
                progress = Math.Clamp(progress, 0.0f, 1.0f);

                // Läbipaistvus kahaneb aja jooksul (hajuob)
                float alpha = 1.0f - progress;

                // 1. VÄLIMINE SPLASH-ALA (Näiteks poolläbipaistev kollane või oranž)
                if (expl.SplashAreaRadius > 0)
                {
                    float currentSplashRadius = expl.SplashAreaRadius * progress;

                    canvas.FillColor = expl.SplashAreaColor.WithAlpha(alpha * 0.7f); // Õrnem kollane
                    canvas.FillCircle(expl.X, expl.Y, currentSplashRadius);
                }

                // 2. SISEMINE PEAMINE PLAHVATUSALA (Näiteks ere oranž/punane tuumik)
                float currentMainRadius = expl.DamageAreaRadius * progress;

                canvas.FillColor = expl.DamageAreaColor.WithAlpha(alpha * 1.0f); // Tugevam oranžikas-punane
                canvas.FillCircle(expl.X, expl.Y, currentMainRadius);

                // Pisike valge sära keskel esimesel paaril kaadril
                if (progress < 0.3f)
                {
                    canvas.FillColor = Colors.White.WithAlpha((0.4f - progress) / 0.4f);
                    canvas.FillCircle(expl.X, expl.Y, currentMainRadius * 0.4f);
                }
            }
        }

        //Teekonna joonistamine
        private void DrawRoad(ICanvas canvas)
        {
            canvas.StrokeColor = _currentStrokeColor;
            canvas.StrokeSize = 40;
            canvas.DrawPath(_cachedRoadPath);
        }

        //Torni joonistamine
        private void DrawTowers(ICanvas canvas)
        {
            //Tornid
            foreach (var tower in Towers)
            {
                canvas.FillColor = tower.TowerColor; // Kasutame torni spetsiifilist värvi
                canvas.FillCircle(tower.X, tower.Y, tower.SizeRadius);

                // Valikuline: Väike äär või täpp keskele, et torn näeks ilusam välja
                canvas.FillColor = tower.DotColor;
                canvas.FillCircle(tower.X, tower.Y, 6);

                HealthBar(canvas, tower.X, tower.Y, tower.MaxHealth, tower.CurrentHealth, tower.SizeRadius);
            }
        }

        //Mürskude joonistamine
        private void DrawProjectiles(ICanvas canvas)
        {
            foreach (var proj in ActiveProjectiles)
            {
                // Arvutame lennu edenemise (0.0 kuni 1.0)
                float total = proj.TotalTravelFrames > 0 ? proj.TotalTravelFrames : 1;
                float progress = 1.0f - ((float)proj.FramesRemaining / total);
                                
                progress = Math.Clamp(progress, 0.0f, 1.0f); // Tagame, et progress jääb vahemikku 0-1

                float sizeMultiplier = (float)Math.Sin(progress * Math.PI); // Arvutame suuruse kordaja (Sinus funktsiooni abil) Sin(0) = 0, Sin(pi/2 [poole peal]) = 1, Sin(pi) = 0

                // Arvutame hetke raadiuse
                float currentRadius = 2 + (10 * sizeMultiplier);


                canvas.FillColor = proj.Color;
                canvas.FillCircle(proj.CurrentX, proj.CurrentY, currentRadius);
            }
        }

        // LOHISTAMISE EELVAADE (Kui mängija lohistab nuppu)
        private void DragPreview(ICanvas canvas)
        {            
            if (IsDragging)
            {
                bool canBuild = BuildService.CanBuildAt(CurrentDragPos.X, CurrentDragPos.Y, DraggingSizeRadius, PathPoints, Towers);

                // Ulatuse ring (poolläbipaistev)
                canvas.FillColor = canBuild ? ValidBuildColor : InvalidBuildColor;
                canvas.FillCircle(CurrentDragPos.X, CurrentDragPos.Y, DraggingRange);

                // Torni eelvaate ring kursori all
                canvas.FillColor = DraggingColor;
                canvas.FillCircle(CurrentDragPos.X, CurrentDragPos.Y, DraggingSizeRadius);
            }
        }

        //Vaenlase joonistamine
        private void DrawEnemy(ICanvas canvas)
        {
            //VAENLASED KOOS ELURIBADEGA
            foreach (var enemy in Enemies)
            {
                if (enemyImage != null)
                {
                    float size = (float)(enemy.SizeRadius * 2);
                    canvas.DrawImage(enemyImage, enemy.X - (size / 2), enemy.Y - (size / 2), size, size);
                }
                else
                {
                    // Vaenlase keha (punane ring)
                    canvas.FillColor = enemy.Color;
                    canvas.FillCircle(enemy.X, enemy.Y, enemy.SizeRadius);
                }                

                HealthBar(canvas, enemy.X, enemy.Y, enemy.MaxHealth, enemy.CurrentHealth, enemy.SizeRadius);
            }
        }

        // Eluriba loomine
        private void HealthBar(ICanvas canvas, float objX, float objY, int objMaxH, int objCurrH, int objRadius)
        {
            // --- ELURIBA ---
            float barWidth = 24;
            float barHeight = 4;
            float barX = objX - barWidth / 2;
            float barY = objY - objRadius;

            // Eluriba taust (punaesel taustal punane/must)
            canvas.FillColor = Colors.Black;
            canvas.FillRectangle(barX, barY, barWidth, barHeight);

            // Praegused elud (roheline riba)
            float maxH = objMaxH > 0 ? objMaxH : 50;
            float healthPercent = (float)objCurrH / maxH;
            healthPercent = Math.Clamp(healthPercent, 0.0f, 1.0f);

            canvas.FillColor = Colors.LimeGreen;
            canvas.FillRectangle(barX, barY, barWidth * healthPercent, barHeight);
        }

        //Joonistab otsetule kiired
        private void DrawDirectfireBeams(ICanvas canvas)
        {
            foreach (var fx in ActiveDirectfireEffects)
            {
                canvas.StrokeColor = fx.Color;
                canvas.StrokeSize = 3; // Joone paksus
                canvas.DrawLine(fx.StartX, fx.StartY, fx.EndX, fx.EndY);
            }
        }

        public void ResetDrawable()
        {
            // Puhastame kaardi objektidest
            isPathGenerated = false;
            _cachedRoadPath = null;
            Towers.Clear();
            Enemies.Clear();
            ActiveProjectiles.Clear();
            ActiveDirectfireEffects.Clear();
            ActiveExplosions.Clear();

            // Taastame mängija algseisu
            CurrentPlayer.Reset();
        }
    }
}