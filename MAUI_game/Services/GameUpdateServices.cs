using MAUI_game.Models;
using MAUI_game.Services.UpdateManagers;

namespace MAUI_game.Services
{
    public class GameUpdateServices
    {
        private readonly List<EnemyModel> _enemies;
        private readonly List<TowerModel> _towers;
        private readonly List<MapPointModel> _pathPoints;
        private readonly PlayerModel _currentPlayer;

        public EnemyManager _enemyManager;
        public WaveManager _waveManager { get; }
        private CombatManager _combatManager;

        private readonly List<DirectfireEffectModel> _activeDirectfires;
        private readonly List<ExplosionEffectModel> _activeExplosions;
        private readonly List<ProjectileModel> _activeProjectiles;

        // Konstruktor, selle abil soome sisse mujalt liste
        public GameUpdateServices(List<EnemyModel> enemies, List<TowerModel> towers, List<MapPointModel> pathPoints, PlayerModel currentPlayer, List<DirectfireEffectModel> activeDirectFireEffects, List<ExplosionEffectModel> activeExplosions, List<ProjectileModel> activeProjectiles)
        {
            _enemies = enemies;
            _towers = towers;
            _pathPoints = pathPoints;
            _currentPlayer = currentPlayer;
            _activeDirectfires = activeDirectFireEffects;
            _activeExplosions = activeExplosions;
            _activeProjectiles = activeProjectiles;

            _enemyManager = new EnemyManager(_enemies, _pathPoints, _currentPlayer);
            _combatManager = new CombatManager(_enemies, _towers, _activeDirectfires, _activeExplosions, activeProjectiles);
            _waveManager = new WaveManager(_enemies, _pathPoints);
        }
        public void UpdateGame(double deltaTime)
        {

            if (_pathPoints == null || _pathPoints.Count < 2) return;

            _enemyManager.UpdateEnemies();
            _combatManager.UpdateTowers();
            _combatManager.UpdateProjectiles();
            _combatManager.UpdateDirectFireEffects();
            _combatManager.UpdateExplosions();
            _waveManager.UpdateWaves(deltaTime);
        }        
    }
}
