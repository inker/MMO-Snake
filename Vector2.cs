namespace Snake
{

    public class Vec2
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        public Vec2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Vec2 v) => X == v.X && Y == v.Y;
        public static Vec2 operator+(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator*(Vec2 v, int a) => new Vec2(v.X * a, v.Y * a);
    }

    public class Vector2
    {
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Vector2 v) => X == v.X && Y == v.Y;
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator *(Vector2 v, float a) => new Vector2(v.X * a, v.Y * a);
    }
}
