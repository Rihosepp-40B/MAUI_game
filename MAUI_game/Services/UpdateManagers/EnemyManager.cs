using MAUI_game.Models;

namespace MAUI_game.Services.UpdateManagers
{
    public class EnemyManager
    {
        private readonly List<EnemyModel> _enemies;
        private readonly List<MapPointModel> _pathPoints;
        private readonly PlayerModel _currentPlayer;
        public event EventHandler OnGameOver;

        public EnemyManager(List<EnemyModel> enemies, List<MapPointModel> pathPoints, PlayerModel currentPlayer)
        {
            _enemies = enemies;
            _pathPoints = pathPoints;
            _currentPlayer = currentPlayer;
        }

        public void UpdateEnemies()
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];

                //Kontrollime kas vastse elud on 0
                if (enemy.CurrentHealth <= 0)
                {
                    // Tagame tasu raha ja skoori näol (näiteks +15 kulda ja +100 skoori)
                    _currentPlayer.Gold += enemy.GiveGold;
                    _currentPlayer.Score += enemy.GiveScore;

                    _enemies.RemoveAt(i);
                    continue;
                }

                // Kui vaenlane on jõudnud teekonna lõppu, eemaldame ta ja võtame elusid
                if (enemy.TargetPointIndex >= _pathPoints.Count)
                {
                    EnemyReachGoal(i);

                    continue;
                }

                EnemyMovement(enemy);
            }
        }

        private void EnemyMovement(EnemyModel enemy)
        {
            var targetPoint = _pathPoints[enemy.TargetPointIndex];

            // Arvutame suuna ja liikumise järgmise punkti poole
            float dx = targetPoint.X - enemy.X;
            float dy = targetPoint.Y - enemy.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance <= enemy.Speed)
            {
                // Jõudis punkti, liigume järgmise teepunkti sihtmärgiks
                enemy.X = targetPoint.X;
                enemy.Y = targetPoint.Y;
                enemy.TargetPointIndex++;
            }
            else
            {
                // Liigume sujuvalt sihtmärgi poole
                enemy.X += (dx / distance) * enemy.Speed;
                enemy.Y += (dy / distance) * enemy.Speed;
            }
        }

        private void EnemyReachGoal(int i)
        {
            _currentPlayer.CurrentHealth--;
            _enemies.RemoveAt(i);

            // KONTROLLIME, KAS ELUD LÕPPESID
            if (_currentPlayer.CurrentHealth <= 0)
            {
                _currentPlayer.CurrentHealth = 0; // Ei luba negatiivseks minna

                OnGameOver?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
