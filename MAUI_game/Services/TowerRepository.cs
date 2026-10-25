using MAUI_game.Models;
using MAUI_game.Resources.Localization;

namespace MAUI_game.Services
{
    public static class TowerRepository
    {
        public static Dictionary<string, TowerTypeModel> GetCatalog()
        {
            return new Dictionary<string, TowerTypeModel>
            {
                {
                    "MG", new TowerTypeModel
                    {
                        Id = "MG",
                        Name = AppResources.MG,
                        Cost = 100,
                        Damage = 4,
                        DamageArea = 1,
                        SplashDamageArea = 0,
                        Range = 90,
                        TravelTime = 0,
                        Health = 200,
                        Firerate = 400,
                        //Salvo = 300,
                        //ReloadTime = 2,
                        Color = Colors.ForestGreen,
                        SizeRadius = 25,
                        IsDirectFire = true,
                    }
                },
            {
                    "Mortar", new TowerTypeModel
                    {
                        Id = "Mortar",
                        Name = AppResources.Mortar,
                        Cost = 250,
                        Damage = 70,
                        DamageArea = 20,
                        SplashDamageArea = 60,
                        Range = 300,
                        TravelTime = 1.5,
                        Health = 75,
                        Firerate = 40,
                        //Salvo = 6,
                        //ReloadTime = 10,
                        Color = Colors.Brown,
                        SizeRadius = 45,
                        IsDirectFire = false,
                    }
                },
            };
        }
    }
}
