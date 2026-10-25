using MAUI_game.Interfaces;
using MAUI_game.Models;

namespace MAUI_game.Services.UpdateManagers
{
    public class CombatManager
    {
        private readonly AudioService _audioService = new AudioService();
        private readonly List<EnemyModel> _enemies;
        private readonly List<TowerModel> _towers;
        private readonly List<DirectfireEffectModel> _activeDirectfires;
        private readonly List<ExplosionEffectModel> _activeExplosions;
        private readonly List<ProjectileModel> _activeProjectiles;
        public CombatManager(
            List<EnemyModel> enemies, 
            List<TowerModel> towers, 
            List<DirectfireEffectModel> activeDirectFireEffects, 
            List<ExplosionEffectModel> activeExplosions, 
            List<ProjectileModel> activeProjectiles
            )
        {
            _enemies = enemies;
            _towers = towers;
            _activeDirectfires = activeDirectFireEffects;
            _activeExplosions = activeExplosions;
            _activeProjectiles = activeProjectiles;
        }

        //Uuendame plahvatust
        public void UpdateExplosions()
        {
            for (int i = _activeExplosions.Count - 1; i >= 0; i--)
            {
                _activeExplosions[i].FramesRemaining--;
                if (_activeExplosions[i].FramesRemaining <= 0)
                {
                    _activeExplosions.RemoveAt(i);
                }
            }
        }

        //Uuendame relva effekte
        public void UpdateDirectFireEffects()
        {
            for (int i = _activeDirectfires.Count - 1; i >= 0; i--)
            {
                _activeDirectfires[i].FramesRemaining--;
                if (_activeDirectfires[i].FramesRemaining <= 0)
                {
                    _activeDirectfires.RemoveAt(i);
                }
            }
        }

        //Mürsu liikumise värskendamine
        public void UpdateProjectiles()
        {
            // MÜRSKUDE uuendamine
            for (int i = _activeProjectiles.Count - 1; i >= 0; i--)
            {
                var proj = _activeProjectiles[i];
                proj.FramesRemaining--;

                // Arvutame mitu protsenti lennust on läbitud (0.0 kuni 1.0)
                float progress = 1.0f - ((float)proj.FramesRemaining / proj.TotalTravelFrames);
                if (progress > 1.0f) progress = 1.0f;

                // Liigutame mürsu visuaalselt algpunktist sihtpunkti poole
                proj.CurrentX = proj.StartX + (proj.TargetX - proj.StartX) * progress;
                proj.CurrentY = proj.StartY + (proj.TargetY - proj.StartY) * progress;

                // Kui lennuaeg on läbi (mürsk jõudis sihtmärgini)
                if (proj.FramesRemaining <= 0)
                {
                    CheckWhatWasHit(proj, _enemies);
                    CheckWhatWasHit(proj, _towers);
                    CreateExplosionData(proj);
                    _activeProjectiles.RemoveAt(i);
                    _audioService.PlaySoundAsync("Explosion.WAV");
                }
            }
        }

        //Plahvatuse andmete seadmine
        private void CreateExplosionData(ProjectileModel proj)
        {
            _activeExplosions.Add(new ExplosionEffectModel
            {
                X = proj.TargetX,
                Y = proj.TargetY,
                DamageAreaRadius = proj.DamageArea > 0 ? proj.DamageArea : 15,
                SplashAreaRadius = proj.SplashDamageArea > 0 ? proj.SplashDamageArea : 30,
            });
        }

        //Kontrollib, mis objektid said pihta
        private void CheckWhatWasHit(ProjectileModel proj, IEnumerable<IDamageable> Targets)
        {

            for (int i = Targets.Count() - 1; i >= 0; i--)
            {
                var target = Targets.ElementAt(i);
                int health = target.CurrentHealth;
                ApplyAreaDamageToObjects(target.X, target.Y, ref health, proj, target.SizeRadius);
                target.CurrentHealth = health;
            }
        }

        //Ala kahju kehtestamine
        private void ApplyAreaDamageToObjects(float objX, float objY, ref int currentHealth, ProjectileModel proj, int objR)
        {
            float dx = objX - proj.TargetX;
            float dy = objY - proj.TargetY;
            float distSq = (float)Math.Sqrt(dx * dx + dy * dy);


            if (proj.DamageArea > 0 && distSq < proj.DamageArea + objR)
            {
                currentHealth -= proj.Damage;
            }

            else if (proj.SplashDamageArea > 0 && distSq < proj.SplashDamageArea + objR)
            {
                    currentHealth -= Math.Max(1, proj.SplashDamage);
            }

            else if (proj.DamageArea == 0 && proj.SplashDamageArea == 0 && distSq <= objR)
            {
                currentHealth -= proj.Damage;
            }
        }


        //Uuendab tornide olekut ja tegevust
        public void UpdateTowers()
        {
            for (int i = _towers.Count - 1; i >= 0; i--)
            {
                var tower = _towers[i];

                if (tower.CurrentHealth <= 0)
                {
                    _towers.RemoveAt(i);
                    continue;
                }
                // Kui Cooldown ei ole null, siis jätka kuni on.
                if (tower.CooldownTimer > 0)
                {
                    tower.CooldownTimer--;
                    tower.SoundCoolDown--;
                    continue;
                }
                FindEnemyToShoot(tower);
            }
        }

        //Leiab vaenlase keda tulistada
        public void FindEnemyToShoot(TowerModel tower)
        {

            for (int i = 0; i < _enemies.Count; i++)
            {
                var enemy = _enemies[i];
                float dx = enemy.X - tower.X;
                float dy = enemy.Y - tower.Y;
                float distToEnemy = (dx * dx) + (dy * dy);

                float maxAllowedDistance = tower.Range + enemy.SizeRadius;
                float maxAllowedDistanceSquared = maxAllowedDistance * maxAllowedDistance;

                if (distToEnemy <= maxAllowedDistanceSquared)
                {
                    if (tower.IsDirectFire)
                    {
                        DirectFireEffect(enemy, tower);
                        DirectFireDamage(enemy, tower);
                        PlayFieringSound(tower, 4);
                    }
                    else
                    {
                        SetupProjectile(tower, enemy);
                        PlayFieringSound(tower, 1);
                    }

                    // Torni tulekiiruse kehtestamine
                    tower.CooldownTimer = tower.AttackSpeedFrames;                                       
                    break;
                }
            }
        }

        public void PlayFieringSound(TowerModel tower, int multiplier)
        {
            if (tower.SoundCoolDown > 0)
            {
                tower.SoundCoolDown--;
            }
            else
            {
                _audioService.PlaySoundAsync(tower.SoundFileName);
                tower.SoundCoolDown = tower.AttackSpeedFrames * multiplier;
            }            
        }

        private void SetupProjectile(TowerModel tower, EnemyModel enemy)
        {
            int splashDamage = tower.Damage / 2;
            // KAUDTULI: loome mürsu, mis jõuab sihtmärgini määratud TravelFramiga
            _activeProjectiles.Add(new ProjectileModel
            {
                StartX = tower.X,
                StartY = tower.Y,
                TargetX = enemy.X,
                TargetY = enemy.Y,
                CurrentX = tower.X, // Alustab tornist
                CurrentY = tower.Y,
                Damage = tower.Damage,
                DamageArea = tower.DamageArea,
                SplashDamageArea = tower.SplashDamageArea,
                SplashDamage = splashDamage,
                TotalTravelFrames = tower.AttackTravelFrames,  // <--- Otse tornilt
                FramesRemaining = tower.AttackTravelFrames,    // <--- Otse tornilt

            });
        }
                
        // Otsetule relva tule loomine
        private void DirectFireEffect(EnemyModel enemy, TowerModel tower)
        {
            // Otsetule relvade visuaalne effekt
            _activeDirectfires.Add(new DirectfireEffectModel
            {
                StartX = tower.X,
                StartY = tower.Y,
                EndX = enemy.X,
                EndY = enemy.Y,
            });            
        }

        private void DirectFireDamage(EnemyModel enemyPrimary, TowerModel tower)
        {
            // Kiire algus- ja lõpp-punkt (tornist peasihtmärgini)
            float x1 = tower.X;
            float y1 = tower.Y;
            float x2 = enemyPrimary.X;
            float y2 = enemyPrimary.Y;

            float lineLengthSquared = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);

            DamageForEachObj(enemyPrimary, tower, x1, y1, x2, y2, lineLengthSquared, _enemies);
            DamageForEachObj(enemyPrimary, tower, x1, y1, x2, y2, lineLengthSquared, _towers);           
        }

        private void DamageForEachObj(EnemyModel enemyPrimary, TowerModel tower, float x1, float y1, float x2, float y2, float lineLengthSquared, IEnumerable<IDamageable> Targets)
        {

            for (int i = Targets.Count() - 1; i >= 0; i--)
            //foreach (var obj in list)
            {
                var obj = Targets.ElementAt(i);
                bool isHit = false;
                // Arvutame objekti projektsiooni kiire lõigule (matemaatiline valem sirglõigu ja punkti vaheliseks kauguseks)
                float t = ((obj.X - x1) * (x2 - x1) + (obj.Y - y1) * (y2 - y1)) / lineLengthSquared;

                // Piirame t väärtuse vahemikku [0, 1], et see jääks torni ja sihtmärgi vahele
                t = Math.Max(0, Math.Min(1, t));

                // Lähim punkt sirglõigul vaenlase suhtes
                float nearestX = x1 + t * (x2 - x1);
                float nearestY = y1 + t * (y2 - y1);

                // Kaugus objekti keskpunktist selle lähima punktini sirgel
                float distDx = obj.X - nearestX;
                float distDy = obj.Y - nearestY;
                float distSq = distDx * distDx + distDy * distDy;

                // Kui vahemaa on väiksem kui vaenlase raadius ruudus, siis kiir tabas teda!
                if (distSq <= obj.SizeRadius * obj.SizeRadius)
                {
                    isHit = true;
                }

                // Kui objekt jääb kiire teele, teeme talle kahju
                if (isHit)
                {
                    if (obj != tower)
                    {
                        if (obj == enemyPrimary)
                        {
                            obj.CurrentHealth -= tower.Damage;
                        }
                        else
                        {
                            obj.CurrentHealth -= tower.Damage / 4;
                        }
                    }
                }
            }
        }
    }
}
