using MAUI_game.Models;

namespace MAUI_game.Services.UpdateManagers
{
    public class WaveManager
    {
        public int currentWave = 0;
        public double timeBetweenWaves = 20.0;
        public double waveTimer = 10.0;          // Ajalugeja järgmise laineni (sekundites) siin = on see millega mäng alustab
        public bool isSpawningWave = false;   // Kas hetkel käib vastaste väljasaatmine platsile
        public double spawnTimer = 0;         // Ajalugeja üksikute vastaste vahel
        public int enemiesRemainingToSpawn = 0;
        public double currentSpawnInterval = 1.5;

        private int enemiesToSpawnAtTime = 0;
        private double multiSpawnTimer = 0.3;

        private readonly List<EnemyModel> _enemies;
        private readonly List<MapPointModel> _pathPoints;

        private static readonly int[] Health = { 250, 200, 150, 100, 90, 50, 30 };
        private static readonly float[] Speed = { 2, 2.5f, 3.5f, 4.5f, 5.5f, 6.5f, 7 };
        private static readonly int[] Size = { 24, 20, 16, 12, 10, 8, 6 };
        private static readonly int[] Gold = { 24, 18, 16, 12, 10, 6, 2 };

        public WaveManager(
            List<EnemyModel> enemies, 
            List<MapPointModel> pathPoints
            )
        {
            _enemies = enemies;
            _pathPoints = pathPoints;
        }

        //Värskendab vastase laineid
        public void UpdateWaves(double deltaTime)
        {
            if (!isSpawningWave)
            {
                // Kui me ei spawni praegu vastaseid, loeme aja järgmise laineni
                waveTimer += deltaTime;
                if (waveTimer >= timeBetweenWaves)
                {
                    isSpawningWave = true;
                    waveTimer = 0;
                    StartNextWave();
                    enemiesToSpawnAtTime = currentWave;
                }
            }
            else
            {
                // Kui laine on alanud, saadame vastaseid välja kindla intervalliga ja gruppides
                spawnTimer += deltaTime;
                if (spawnTimer >= currentSpawnInterval)
                {
                    multiSpawnTimer += deltaTime;
                    if (multiSpawnTimer >= 0.1)
                    {
                        multiSpawnTimer = 0;
                        SpawnSingleEnemy();
                        enemiesToSpawnAtTime--;
                    }

                    if (enemiesToSpawnAtTime <= 0)
                    {
                        spawnTimer = 0;
                        enemiesToSpawnAtTime = currentWave;
                        multiSpawnTimer = 0.3;
                    }
                }

                if (enemiesRemainingToSpawn <= 0)
                {
                    isSpawningWave = false; // Laine on platsile saadetud
                }
            }
        }

        //Vastase loomine
        private void SpawnSingleEnemy()
        {
            if (_pathPoints.Count == 0) return;

            var startPoint = _pathPoints[0];

            var newEnemy = new EnemyModel
            {
                X = startPoint.X,
                Y = startPoint.Y,
            };                

            // Alates määratud kogusest vastaset tehakse vanlased juhuslikus valikus
            if (currentWave * 10 >= 50)
            {
                int randomNumber = Random.Shared.Next(0, Size.Length);

                if (enemiesRemainingToSpawn < Gold[randomNumber])
                {
                    randomNumber = Size.Length - 1; // kõige väiksem vastane
                }
                int addedHealth = 0;
                float addedSpeed = 0;

                if (currentWave >= 10)
                {
                    addedHealth = (currentWave / 5 - 1) * 5;
                    addedSpeed = (currentWave / 5 - 1) * 0.1f;
                }

                newEnemy.MaxHealth = Health[randomNumber] + addedHealth;
                newEnemy.CurrentHealth = newEnemy.MaxHealth;
                newEnemy.Speed = Speed[randomNumber] + addedSpeed;
                newEnemy.SizeRadius = Size[randomNumber];
                newEnemy.GiveGold = Gold[randomNumber];
                newEnemy.GiveScore = Gold[randomNumber];
            }

            enemiesRemainingToSpawn = enemiesRemainingToSpawn - newEnemy.GiveGold;
            _enemies.Add(newEnemy);
        }

        //Järgise laine seadmine
        private void StartNextWave()
        {
            currentWave++;

            //laine on 100 kulda vastast, iga järgmine laine suureneb 100 võrra
            enemiesRemainingToSpawn = currentWave * 100;

            // Iga lainega suureneb ka vastaste gruppide tuleku kiirus
            currentSpawnInterval = Math.Max(0.2, 1.5 - ((currentWave - 1) * 0.05));
        }

        public void ResetWave()
        {
            // Nullime vaenlase tuleku andmed
            currentWave = 0;
            waveTimer = 10.0;
            isSpawningWave = false;
            spawnTimer = 0;
            enemiesRemainingToSpawn = 0;
            currentSpawnInterval = 1.5;
        }
    }
}
