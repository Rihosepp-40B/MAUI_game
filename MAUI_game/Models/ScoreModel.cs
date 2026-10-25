using SQLite;

namespace MAUI_game.Models
{
    public class ScoreModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string PlayerName { get; set; }
        public int Score { get; set; }
        public DateTime DatePlayed { get; set; } = DateTime.Now;
    }
}
