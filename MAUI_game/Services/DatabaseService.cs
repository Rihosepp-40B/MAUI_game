using MAUI_game.Models;
using SQLite;

namespace MAUI_game.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        private async Task Init()
        {
            if (_database != null)
                return;

            // Määrame andmebaasi faili asukoha seadmes
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "towergame.db3"); // Kuskil truvaline kaust /data/user/0/com.companyname.maui_game/files/towergame.db3

            _database = new SQLiteAsyncConnection(dbPath);

            // Loome tabeli, kui seda pole veel olemas
            await _database.CreateTableAsync<ScoreModel>();
        }

        // Loe kõiki kirjeid (näiteks edetabeli kuvamiseks)
        public async Task<List<ScoreModel>> GetScoresAsync()
        {
            await Init();
            // Tagastame tulemused kahanevas järjekorras (suurem skoor eespool)
            return await _database.Table<ScoreModel>().OrderByDescending(s => s.Score).ToListAsync();
        }

        // Lisa uus kirje
        public async Task<int> SaveScoreAsync(ScoreModel score)
        {
            await Init();
            return await _database.InsertAsync(score);
        }

        // Muuda olemasolevat kirjet
        public async Task<int> UpdateScoreAsync(ScoreModel score)
        {
            await Init();
            return await _database.UpdateAsync(score);
        }

        // Kustuta kirje
        public async Task<int> DeleteScoreAsync(ScoreModel score)
        {
            await Init();
            return await _database.DeleteAsync(score);
        }
    }
}
