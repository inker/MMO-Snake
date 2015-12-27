using System.Collections.Generic;

namespace Snake
{
    public class Player
    {
        public List<Vec2> Snake;
        public int ColorNum;
        public int Score;
        public Player(List<Vec2> snake, int colorNum, int score = 0)
        {
            Snake = snake;
            ColorNum = colorNum;
            Score = score;
        }

        public Player(int score = 0) {
            Score = score;
        }
    }
}
