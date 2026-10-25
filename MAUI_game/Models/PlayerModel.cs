namespace MAUI_game.Models
{
    public class PlayerModel
    {
        public string Name { get; set; } = "Player";
        public int CurrentHealth { get; set; } = 10;
        public int MaxHealth { get; set; } = 10;
        public int Gold { get; set; } = 390;
        public int Score { get; set; } = 0;
        public int TotalScore => Gold + Score;

        public void Reset()
        {
            CurrentHealth = 10;
            Gold = 390;
            Score = 0;
            Name = "Player";
        }
    }
}
