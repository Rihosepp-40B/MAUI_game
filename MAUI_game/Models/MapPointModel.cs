namespace MAUI_game.Models
{
    public class MapPointModel
    {
        public float X { get; set; }
        public float Y { get; set; }

        public MapPointModel(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}