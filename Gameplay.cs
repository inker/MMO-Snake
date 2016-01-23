using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Snake
{
    public class Gameplay : Player
    {
        public int FoodPoints = 10;
        public int NitroPenalty = 1;
        public int InitialSnakeLength = 4;
        int _initialSpeed = 20;

        public int InitialSpeed
        {
            get { return _initialSpeed; }
            set
            {
                _initialSpeed = value;
                SpeedChange(this, new EventArgs());
            }
        }

        public event EventHandler OnMessage
        {
            add { Server.OnMessage += value; }
            remove { Server.OnMessage -= value; }
        }

        public event EventHandler GridResize;
        public event EventHandler SpeedChange;

        private Vec2 _grid = new Vec2(70, 50);
        public Vec2 Grid
        {
            get { return _grid; }
            set
            {
                _grid = value;
                GridResize(this, new EventArgs());
            }
        }
        public bool IsOver;
        public Vec2 Food;
        public Dictionary<byte, Player> Opponents = new Dictionary<byte, Player>();

        int _score;
        public new int Score{
            get { return _score; }
            set
            {
                Server.ReportSituation();
                _score = value;
            }
        }

        ClientServer Server;
        byte Step = 0;
        int Direction = 39; // Left = 37, Up = 38, Right = 39, Down = 40
        bool FreeNitro;

        public Gameplay() : base(new List<Vec2>(), System.Drawing.Color.Red)
        {
            Server = new ClientServer(this);
        }

        public void Start()
        {
            IsOver = false;
            Score >>= 1;
            Opponents.Clear();
            Direction = 39;
            Nitro = false;
            FreeNitro = Score == 0;
            var x = Util.Random(InitialSnakeLength, Grid.X - InitialSnakeLength * 2);
            var y = Util.Random(InitialSnakeLength, Grid.Y - InitialSnakeLength * 2);
            MakeSnake(new Vec2(x, y));
            Server.Connect();
            GenerateFood();
        }

        private void MakeSnake(Vec2 headPos)
        {
            Snake.Clear();
            for (int i = 0; i < InitialSnakeLength; ++i)
            {
                Snake.Add(new Vec2(headPos.X - i, headPos.Y));
            }
        }

        private void GenerateFood()
        {
            do
            {
                Food = new Vec2(Util.Random(0, Grid.X), Util.Random(0, Grid.Y));
            }
            while (Food.X == Snake[0].X || Food.Y == Snake[0].Y);
        }

        public void Update()
        {
            if (IsOver)
            {
                Server.CloseSocket("Connection closed. You've killed yourself.");
                return;
            }
            Server.ReportSituation();
            UpdateSnake();
            if (Nitro && Score > 0 && ++Step % 2 > 0 && !FreeNitro && (Score -= NitroPenalty) < 1)
            {
                Nitro = false;
            }
        }

        private void UpdateSnake()
        {
            var oldHead = Snake[0];
            var head = new Vec2(oldHead.X, oldHead.Y);
            switch (Direction)
            {
                case 37: --head.X; break;
                case 38: --head.Y; break;
                case 39: ++head.X; break;
                case 40: ++head.Y; break;
            }

            // snake has run into the wall
            if (head.X < 0 || head.X >= Grid.X || head.Y < 0 || head.Y >= Grid.Y)
            {
                IsOver = true;
                return;
            }
            // snake has run into itself
            foreach (var part in Snake)
            {
                if (head.Equals(part))
                {
                    IsOver = true;
                    return;
                }
            }
            // snake has run into an opponent
            foreach (var opponent in Opponents.Values)
            {
                try
                {
                    foreach (var part in opponent.Snake)
                    {
                        if (head.Equals(part))
                        {
                            IsOver = true;
                            return;
                        }
                    }
                }
                catch (Exception e) { }
            }
            // snake has run into food
            if (head.Equals(Food))
            {
                GenerateFood();
                Server.ReportNewFood();
                Score += FoodPoints;
                FreeNitro = false;
            }
            else
            {
                Snake.RemoveAt(Snake.Count - 1);
            }
            Snake.Insert(0, head);
        }

        public void HandleKeyDown(Keys key)
        {
            if (key == Keys.Enter)
            {
                if (IsOver)
                {
                    Start();
                }
            }
            else if (key != Keys.Space)
            {
                var head = Snake[0];
                var next = Snake[Snake.Count > 1 ? 1 : 0];
                var keyCode = (int)key;
                if ((key == Keys.Left || key == Keys.Right) && head.X == next.X || (key == Keys.Up || key == Keys.Down) && head.Y == next.Y)
                {
                    Direction = keyCode;
                }
            }
            else if (Score > 0 || FreeNitro)
            {
                Nitro = true;
            }
        }

        public void HandleKeyUp(Keys key)
        {
            if (key == Keys.Space)
            {
                Nitro = false;
            }
        }


        public Player GetOrMakeOpponent(byte id, int score, bool nitro)
        {
            Player opponent;
            if (Opponents.TryGetValue(id, out opponent))
            {
                opponent.Score = score;
                opponent.Nitro = nitro;
                opponent.Snake.Clear();
            }
            else
            {
                opponent = new Player(new List<Vec2>(), Color.Gray, score, nitro);
                Opponents.Add(id, opponent);
            }
            return opponent;
        }
    }
}