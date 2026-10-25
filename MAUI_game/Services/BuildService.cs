using MAUI_game.Models;

namespace MAUI_game.Services
{
    public static class BuildService
    {
        // Kontrollib, kas antud koordinaat (x, y) on lubatud ehitusalal        
        public static bool CanBuildAt(float x, float y, int sizeRadius, List<MapPointModel> PathPoints, List<TowerModel> Towers)
        {
            if (x - sizeRadius < 0 || x + sizeRadius > CanvaSizeModel.CanvasWidth || y - sizeRadius < 0 || y + sizeRadius > CanvaSizeModel.CanvasHeight)
            {
                return false; // Torni serv ulatub mängualast välja!
            }

            if (!CheckIfRoadIsClose(x, y, sizeRadius, PathPoints) || !CheckIfTowerIsClose(x, y, sizeRadius, Towers))
            { return false; }
            return true;

        }

        //Kontrollib kas torn on lähedal (ehitamisel)
        private static bool CheckIfTowerIsClose(float x, float y, int sizeRadius, List<TowerModel> Towers)
        {
            // Kontrollib teiste tornide lähedust (olemasolev + ehitatava suuruste summa)
            foreach (var tower in Towers)
            {
                float dx = tower.X - x;
                float dy = tower.Y - y;
                float distToTower = (float)Math.Sqrt(dx * dx + dy * dy);

                float minAllowedDistance = tower.SizeRadius + sizeRadius;

                if (distToTower < minAllowedDistance)
                {
                    return false; // Liiga lähedal teisele tornile
                }
            }

            return true; // Ei ole tee või teisele tornile liiga lähedal, siis saab ehitada
        }

        //Kontrollib kas tee on lähedal (ehitamisel)
        private static bool CheckIfRoadIsClose(float x, float y, int sizeRadius, List<MapPointModel> PathPoints)
        {
            float roadThreshold = 15f + sizeRadius; // Kui lähedale teele tohib ehitada

            // Kontrollib teekonna lähedust
            for (int i = 0; i < PathPoints.Count - 1; i++)
            {
                var p1 = PathPoints[i];
                var p2 = PathPoints[i + 1];

                float distance = PointToSegmentDistance(x, y, p1.X, p1.Y, p2.X, p2.Y);
                if (distance < roadThreshold)
                {
                    return false; // Liiga lähedal teele, siia ei saa ehitada!
                }
            }
            return true;
        }

        //Punktide vahelise sirglõigu arvutamine
        private static float PointToSegmentDistance(float px, float py, float x1, float y1, float x2, float y2)
        {
            float l2 = (x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1);
            if (l2 == 0) return (float)Math.Sqrt((px - x1) * (px - x1) + (py - y1) * (py - y1));

            float t = ((px - x1) * (x2 - x1) + (py - y1) * (y2 - y1)) / l2;
            t = Math.Max(0, Math.Min(1, t));

            float projX = x1 + t * (x2 - x1);
            float projY = y1 + t * (y2 - y1);

            return (float)Math.Sqrt((px - projX) * (px - projX) + (py - projY) * (py - projY));
        }
    }
}
