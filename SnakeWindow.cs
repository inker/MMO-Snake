using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebSocket4Net;

namespace Snake
{
    public partial class SnakeWindow : Form
    {

        const int InitialSnakeLength = 4;
        Vec2 MaxWindowSize = new Vec2(700, 500);
        const int gridSize = 50;
        const int Speed = 20; // 20 tiles/s
        static Brush[] Colors = {
            Brushes.Red,
            new SolidBrush(Color.FromArgb(0, 64, 255)),
            Brushes.Green,
            Brushes.Yellow,
            Brushes.Orange,
            Brushes.Cyan,
            Brushes.Magenta
        };

        Vec2 Grid;
        Vec2 TileSize;
        List<Vec2> Snake = new List<Vec2>();
        bool GameOver;
        int Score;

        int Direction; // Down = 0, Left = 1, Right = 2, Up = 3
        Vec2 FoodPiece;

        public PictureBox Canvas;
        Timer GameTimer = new Timer();

        int ColorNum;
        const string ServerURL = "ws://tron-inker.c9.io";
        WebSocket Socket;
        Dictionary<byte, Player> Opponents = new Dictionary<byte, Player>();

        public SnakeWindow()
        {
            InitializeComponent();
            int b = Math.Min(MaxWindowSize.X, MaxWindowSize.Y);
            TileSize = new Vec2(b / gridSize, b / gridSize);
            Grid = new Vec2(MaxWindowSize.X / TileSize.X, MaxWindowSize.Y / TileSize.Y);
            Canvas.Width = TileSize.X * Grid.X;
            Canvas.Height = TileSize.Y * Grid.Y;
            Width = Canvas.Width + 50;
            Height = Canvas.Height + 100;
            KeyDown += OnKeyPressed;
            Canvas.Paint += new PaintEventHandler(Repaint);

            GameTimer.Interval = 1000 / Speed;
            GameTimer.Tick += UpdateGame;
            GameTimer.Start();
            StartGame();
        }

        private void OnKeyPressed(object sender, KeyEventArgs e)
        {
            var value = e.KeyCode;
            var head = Snake[0];
            var first = Snake[1];
            if (value == Keys.Right)
            {
                if (Snake.Count < 2 || head.X == first.X)
                    Direction = 2;
            }
            else if (value == Keys.Left)
            {
                if (Snake.Count < 2 || head.X == first.X)
                    Direction = 1;
            }
            else if (value == Keys.Up)
            {
                if (Snake.Count < 2 || head.Y == first.Y)
                    Direction = 3;
            }
            else if (value == Keys.Down)
            {
                if (Snake.Count < 2 || head.Y == first.Y)
                    Direction = 0;
            }
            else if (GameOver && value == Keys.Enter)
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            GameOver = false;
            Opponents.Clear();
            Direction = 2;
            Score = 0;
            var rand = new Random();
            var x = rand.Next(InitialSnakeLength, Grid.X - InitialSnakeLength * 2);
            var y = rand.Next(InitialSnakeLength, Grid.Y - InitialSnakeLength * 2);
            MakeSnake(new Vec2(x, y));
            ConnectToServer();
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

        private void ConnectToServer()
        {
            Socket = new WebSocket(ServerURL);
            Socket.Opened += (s, e) => ReportSituation();
            Socket.Error += HandleSocketError;
            Socket.MessageReceived += HandleMessage;
            Socket.Open();
        }

        private void HandleSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            if (Socket.State == WebSocketState.Open)
                Socket.Close();
            ConnectToServer();
        }

        private void HandleMessage(object sender, MessageReceivedEventArgs e)
        {
            var msg = e.Message;
            if (msg.StartsWith("food:"))
            {
                var numStrArr = msg.Substring(5).Split(',');
                FoodPiece.X = byte.Parse(numStrArr[0]);
                FoodPiece.Y = byte.Parse(numStrArr[1]);
            }
            else if (msg.StartsWith("color:"))
            {
                ColorNum = int.Parse(msg.Substring(6));
            }
            else if (msg.StartsWith("exit:"))
            {
                byte id = byte.Parse(msg.Substring(5));
                Opponents.Remove(id);
            }
            else
            {
                byte[] bytes = msg.Select(c => (byte)c).ToArray();
                byte id = bytes[0];
                int score = bytes[1];
                int color = bytes[2];
                Player opponent;
                if (Opponents.TryGetValue(id, out opponent))
                {
                    opponent.Score = score;
                    opponent.Snake.Clear();
                }
                else
                {
                    opponent = new Player(new List<Vec2>(), color, score);
                    Opponents.Add(id, opponent);
                }
                var oppSnake = opponent.Snake;
                for (int i = 3; i < bytes.Length; i += 2)
                {
                    oppSnake.Add(new Vec2(bytes[i], bytes[i + 1]));
                }
            }
        }

        private void ReportSituation()
        {
            var packet = new List<byte>(Snake.Count << 1);
            packet.Add((byte)Score);
            packet.Add((byte)ColorNum);
            foreach (var p in Snake)
            {
                packet.Add((byte)p.X);
                packet.Add((byte)p.Y);
            }
            if (Socket.State == WebSocketState.Open)
            {
                Socket.Send(packet.ToArray(), 0, packet.Count);
            }
        }

        private void ReportNewFood()
        {
            if (Socket.State == WebSocketState.Open)
            {
                Socket.Send(string.Format("food:{0},{1}", FoodPiece.X, FoodPiece.Y));
            }

        }

        private void GenerateFood()
        {
            Random random = new Random();
            FoodPiece = new Vec2(random.Next(0, Grid.X), random.Next(0, Grid.Y));
        }

        private void UpdateGame(object sender, EventArgs e)
        {
            if (!GameOver)
            {
                ReportSituation();
                UpdateSnake();
            }
            else if (Socket.State == WebSocketState.Open)
            {
                Socket.Close();
            }
            Canvas.Invalidate();
        }

        private void UpdateSnake()
        {
            var oldHead = Snake[0];
            var head = new Vec2(oldHead.X, oldHead.Y);
            switch (Direction)
            {
                case 0: ++head.Y; break;
                case 1: --head.X; break;
                case 2: ++head.X; break;
                case 3: --head.Y; break;
            }

            Snake.Insert(0, head);

            // snake has run into the wall
            if (head.X < 0 || head.X >= Grid.X || head.Y < 0 || head.Y >= Grid.Y)
            {
                GameOver = true;
                return;
            }
            // snake has run into itself
            for (int j = 1; j < Snake.Count; ++j)
            {
                if (head.X == Snake[j].X && head.Y == Snake[j].Y)
                {
                    GameOver = true;
                    return;
                }
            }
            // snake has run into an opponent
            foreach (var opponent in Opponents.Values)
            {
                if (opponent.Snake == null) continue;
                foreach (var part in opponent.Snake)
                {
                    if (head.X == part.X && head.Y == part.Y)
                    {
                        GameOver = true;
                        return;
                    }
                }
            }
            // snake has run into food
            if (head.X == FoodPiece.X && head.Y == FoodPiece.Y)
            {
                GenerateFood();
                ReportNewFood();
                ++Score;
            }
            else
            {
                Snake.RemoveAt(Snake.Count - 1);
            }
        }

        private void Repaint(object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;
            string scoreMsg = "Score: " + Score;

            if (GameOver)
            {
                string gameOverMsg = "Game over.";
                string newGameMsg = "Press Enter to restart the game";
                int centerWidth = Canvas.Width / 2;
                SizeF msgSize = canvas.MeasureString(gameOverMsg, Font);
                var msgPoint = new PointF(centerWidth - msgSize.Width / 2, 16);
                canvas.DrawString(gameOverMsg, Font, Brushes.White, msgPoint);
                msgSize = canvas.MeasureString(scoreMsg, Font);
                msgPoint = new PointF(centerWidth - msgSize.Width / 2, 32);
                canvas.DrawString(scoreMsg, Font, Brushes.White, msgPoint);
                msgSize = canvas.MeasureString(newGameMsg, Font);
                msgPoint = new PointF(centerWidth - msgSize.Width / 2, 48);
                canvas.DrawString(newGameMsg, Font, Brushes.White, msgPoint);
            }
            else
            {
                foreach (var part in Snake)
                {
                    var rect = new Rectangle(part.X * TileSize.X, part.Y * TileSize.Y, TileSize.X, TileSize.Y);
                    canvas.FillRectangle(Colors[ColorNum], rect);
                }
                int textY = 4;
                canvas.DrawString("You: " + Score, this.Font, Colors[ColorNum], new PointF(4, textY));
                foreach (var pair in Opponents)
                {
                    var opponent = pair.Value;
                    var oppColor = Colors[opponent.ColorNum];
                    try
                    {

                        foreach (var part in opponent.Snake)
                        {
                            var rect = new Rectangle(part.X * TileSize.X, part.Y * TileSize.Y, TileSize.X, TileSize.Y);
                            canvas.FillRectangle(oppColor, rect);
                        }
                        
                }
                catch (Exception exception)
                { }
                var str = string.Format("Opponent {0}: {1}", pair.Key, opponent.Score);
                    canvas.DrawString(str, this.Font, oppColor, new PointF(4, textY += 16));
                }
                var foodRect = new Rectangle(FoodPiece.X * TileSize.X, FoodPiece.Y * TileSize.Y, TileSize.X, TileSize.Y);
                canvas.FillRectangle(Brushes.White, foodRect);
            }
        }
    }
}
