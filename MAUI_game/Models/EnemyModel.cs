using MAUI_game.Interfaces;

namespace MAUI_game.Models
{
    public class EnemyModel : IDamageable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int TargetPointIndex { get; set; } = 1;
        public int CurrentHealth { get; set; } = 95;
        public int MaxHealth { get; set; } = 95;
        public float Speed { get; set; } = 4.5f;
        public int SizeRadius { get; set; } = 12;
        public Color Color { get; set; } = Colors.DarkRed;
        public int GiveScore { get; set; } = 10;
        public int GiveGold { get; set; } = 10;
    }
}
