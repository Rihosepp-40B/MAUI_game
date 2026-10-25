using MAUI_game.Interfaces;

namespace MAUI_game.Models
{
    public class ExplosionEffectModel
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float DamageAreaRadius { get; set; }
        public float SplashAreaRadius { get; set; }

        public int TotalFrames { get; set; } = 15;
        public int FramesRemaining { get; set; } = 15;
        public Color DamageAreaColor { get; set; } = Colors.OrangeRed;
        public Color SplashAreaColor { get; set; } = Colors.Gold;
    }
}
