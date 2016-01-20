using System.Collections.Generic;

namespace Snake
{
    public class Player
    {
        public List<Vec2> Snake;
        public int ColorNum;
        public int Score;
        public bool Nitro;
        public Player(List<Vec2> snake, int colorNum, int score = 0, bool nitro = false)
        {
            Snake = snake;
            ColorNum = colorNum;
            Score = score;
            Nitro = nitro;
        }

        public Player(int score = 0) {
            Score = score;
        }
    }
}
