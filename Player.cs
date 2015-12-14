using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Snake
{
    class Player
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
