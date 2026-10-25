namespace MAUI_game.Interfaces
{
    public interface IDamageable
    {
        float X { get; set; }
        float Y { get; set; }
        int CurrentHealth { get; set; }
        int SizeRadius { get; set; }
    }
}
