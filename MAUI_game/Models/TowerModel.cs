using MAUI_game.Interfaces;

namespace MAUI_game.Models
{
    public class TowerModel : IDamageable
    {
        // Iga uue ehitatud torni unikaalne ID ja asukoht
        public string InstanceId { get; set; } = Guid.NewGuid().ToString();

        public float X { get; set; }
        public float Y { get; set; }

        // TowerTypest pärnievad andmed
        //Nimi ja ID
        public string TowerTypeId { get; set; }
        //public string TowerTypeName { get; set; }

        // Torni tehnilised andmed
        public int Cost { get; set; }
        public int Damage { get; set; }
        public int DamageArea { get; set; }
        public int SplashDamageArea { get; set; }
        public int Range { get; set; }
        public double TravelTime { get; set; }
        public int MaxHealth { get; set; }
        public int Firerate { get; set; }
        public Color TowerColor { get; set; }
        public int SizeRadius { get; set; }

        public bool IsDirectFire { get; set; }

        // Ehitatud torni kalkuleeritud või muutunud andmed
        // Ründekiiruse jaoks vajalikud muutujad tornil:
        public int CooldownTimer { get; set; } = 0;
        public int AttackSpeedFrames { get; set; } // Mitu kaadrit oodatakse kahe lasu vahel
        public int AttackTravelFrames { get; set; } = 0;

        public int CurrentHealth { get; set; }

        public float RangeSquared => Range * Range;
        public Color DotColor { get; set; } = Colors.White;

        public string SoundFileName { get; set; }
        public int SoundCoolDown { get; set; } = 0;
    }
}
