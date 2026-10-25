using MAUI_game.Interfaces;

namespace MAUI_game.Models
{
    public class DirectfireEffectModel
    {
        public float StartX { get; set; }
        public float StartY { get; set; }
        public float EndX { get; set; }
        public float EndY { get; set; }
        public int FramesRemaining { get; set; } = 3; // Püsib ekraanil 3 kaadrit (~0.1 sek)
        public Color Color { get; set; } = Colors.Orange;
    }
}
