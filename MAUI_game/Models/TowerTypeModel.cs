namespace MAUI_game.Models
{
    public class TowerTypeModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }
        public int Damage { get; set; }
        public int DamageArea { get; set; }
        public int SplashDamageArea { get; set; }
        public int Range { get; set; }
        public double TravelTime { get; set; }
        public int Health { get; set; }
        public int Firerate { get; set; }
        //public int Salvo {  get; set; }
        //public int ReloadTime { get; set; }
        public Color Color { get; set; }
        public int SizeRadius { get; set; }

        public bool IsDirectFire {  get; set; }
        public string SoundFileName { get; set; }
    }
}
