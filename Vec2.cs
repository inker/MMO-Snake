using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snake
{
    public class Vec2
    {
        public int X { get; set; } // X Coordinate for Snake Part
        public int Y { get; set; } // Y Coordinate for Snake Part

        // Constructor
        public Vec2()
        {
            X = 0;
            Y = 0;
        }

        public Vec2(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
