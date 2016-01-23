using System.Collections.Generic;
using System.Drawing;

namespace Snake
{
    public class Player
    {
        public List<Vec2> Snake;
        public Color Color;
        public int Score;
        public bool Nitro;
        public Player(List<Vec2> snake, Color color, int score = 0, bool nitro = false)
        {
            Snake = snake;
            Color = color;
            Score = score;
            Nitro = nitro;
        }

        public Player(int score = 0) {
            Score = score;
        }
    }
}
