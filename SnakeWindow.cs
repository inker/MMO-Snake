using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
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
        static Brush[] Colors = new uint[] {
            0xff0000, 0x0040ff, 0x008000, 0xffff00, 0xff8000, 0x00ffff, 0xff00ff
        }.Select(i => new SolidBrush(Color.FromArgb((int)(i + 0xff000000)))).ToArray();

        Vec2 Grid;
        int TileSize;
        List<Vec2> Snake = new List<Vec2>();
        bool GameOver;
        int Score;

        int Direction; // Down = 0, Left = 1, Right = 2, Up = 3
        Vec2 Food;

        PictureBox Canvas;
        Timer GameTimer = new Timer();

        int ColorNum;
        const string ServerURL = "ws://tron-inker.c9.io";
        WebSocket Socket;
        Dictionary<byte, Player> Opponents = new Dictionary<byte, Player>();

        public SnakeWindow()
        {
            InitializeComponent();
            int b = Math.Min(MaxWindowSize.X, MaxWindowSize.Y);
            TileSize = b / gridSize;
            Grid = new Vec2(MaxWindowSize.X / TileSize, MaxWindowSize.Y / TileSize);
            Canvas.Width = Grid.X * TileSize;
            Canvas.Height = Grid.Y * TileSize;
            Width = Canvas.Width + 50;
            Height = Canvas.Height + 60;
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
            this.Text = "Connecting to server...";
            Socket = new WebSocket(ServerURL);
            Socket.Opened += OnSocketOpen;
            Socket.Error += HandleSocketError;
            Socket.MessageReceived += HandleMessage;
            Socket.Closed += (s, e) => this.Text = "Connection to server lost. You've probably killed yourself.";
            Socket.Open();
        }

        private void OnSocketOpen(object sender, EventArgs e)
        {
            this.Text = "Connection to server established";
            Score = 0;
            ReportSituation();
        }

        private void HandleSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            this.Text = string.Format("Error while connecting to server ({0}). Retrying...", e.Exception.Message);
            if (Socket.State == WebSocketState.Open)
            {
                Socket.Close();
            }
            ConnectToServer();
        }

        private void HandleMessage(object sender, MessageReceivedEventArgs e)
        {
            var msg = e.Message;
            if (msg.StartsWith("food:"))
            {
                var numStrArr = msg.Substring(5).Split(',');
                Food.X = byte.Parse(numStrArr[0]);
                Food.Y = byte.Parse(numStrArr[1]);
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
                    opponent.ColorNum = color;
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
                Socket.Send(string.Format("food:{0},{1}", Food.X, Food.Y));
            }
        }

        private void GenerateFood()
        {
            Random random = new Random();
            Food = new Vec2(random.Next(0, Grid.X), random.Next(0, Grid.Y));
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



            // snake has run into the wall
            if (head.X < 0 || head.X >= Grid.X || head.Y < 0 || head.Y >= Grid.Y)
            {
                GameOver = true;
                return;
            }
            // snake has run into itself
            foreach (var part in Snake)
            {
                if (head.Equals(part))
                {
                    GameOver = true;
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
                            GameOver = true;
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
                ReportNewFood();
                ++Score;
            }
            else
            {
                Snake.RemoveAt(Snake.Count - 1);
            }
            Snake.Insert(0, head);
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
                var tileVec = new Vec2(TileSize, TileSize);
                foreach (var part in Snake)
                {
                    DrawRectangle(canvas, part.ScaleBy(TileSize), tileVec, Colors[ColorNum]);
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
                            DrawRectangle(canvas, part.ScaleBy(TileSize), tileVec, oppColor);
                        }
                    }
                    catch (Exception exception)
                    { }
                    var str = string.Format("Opponent {0}: {1}", pair.Key, opponent.Score);
                        canvas.DrawString(str, this.Font, oppColor, new PointF(4, textY += 16));
                }
                DrawRectangle(canvas, Food.ScaleBy(TileSize), new Vec2(TileSize, TileSize), Brushes.White);
            }
        }

        private static void DrawRectangle(Graphics canvas, Vec2 topLeft, Vec2 size, Brush color)
        {
            var rect = new Rectangle(topLeft.X, topLeft.Y, size.X, size.Y);
            canvas.FillRectangle(color, rect);
        }
    }
}
