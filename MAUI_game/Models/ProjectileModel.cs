namespace MAUI_game.Models
{
    public class ProjectileModel
    {
        // Lähtepunkt (kust tornist mürsk teele läks)
        public float StartX { get; set; }
        public float StartY { get; set; }

        // Sihtpunkt (kuhu vaenlase pihta see lendab)
        public float TargetX { get; set; }
        public float TargetY { get; set; }

        // Praegune asukoht kaardil (mida igas kaadris uuendatakse)
        public float CurrentX { get; set; }
        public float CurrentY { get; set; }


        public int Damage { get; set; }
        public int DamageArea { get; set; }
        public int SplashDamage { get; set; }
        public int SplashDamageArea { get; set; } // Kas teeb piirkonnakahju?

        // Lennu kestvuse haldamiseks
        public int TotalTravelFrames { get; set; }
        public int FramesRemaining { get; set; }  // Mitu kaadrit on tabamuseni jäänud

        public Color Color { get; set; } = Colors.Black;
    }
}
