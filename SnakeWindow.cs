using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Snake
{
    public partial class SnakeWindow : Form
    {

        static Vec2 MaxWindowSize = new Vec2(700, 500);
        const int gridSize = 50;

        static Brush[] Colors = new uint[] {
            0xff0000, 0x0040ff, 0x008000, 0xffff00, 0xff8000, 0x00ffff, 0xff00ff
        }.Select(i => new SolidBrush(Color.FromArgb((int)(i + 0xff000000)))).ToArray();

        Game Game;

        PictureBox Canvas;
        int TileSize;
        Timer GameTimer = new Timer();

        public SnakeWindow()
        {
            InitializeComponent();
            int b = Math.Min(MaxWindowSize.X, MaxWindowSize.Y);
            TileSize = b / gridSize;
            var Grid = new Vec2(MaxWindowSize.X / TileSize, MaxWindowSize.Y / TileSize);

            Canvas.Width = Grid.X * TileSize;
            Canvas.Height = Grid.Y * TileSize;
            Width = Canvas.Width + 50;
            Height = Canvas.Height + 60;

            Canvas.Paint += new PaintEventHandler(Repaint);

            Game = new Game(Grid);
            Game.OnMessage += (s, e) => this.Text = e.ToString();
            KeyDown += (s, e) => Game.HandleKeyDown(e.KeyCode);
            KeyUp += (s, e) => Game.HandleKeyUp(e.KeyCode);
            Game.Start();

            GameTimer.Interval = 50;
            GameTimer.Tick += UpdateGame;
            GameTimer.Start();
        }

        private void UpdateGame(object sender, EventArgs e)
        {
            if (GameTimer.Interval < 50)
            {
                if (!Game.Nitro)
                {
                    GameTimer.Interval = 50;
                }
            }
            else if (Game.Nitro)
            {
                GameTimer.Interval = 25;
            }
            Game.Update();
            Canvas.Invalidate();
        }

        private void Repaint(object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;

            if (Game.IsOver)
            {
                int centerWidth = Canvas.Width / 2;
                int y = 0;
                foreach (var s in new string[] { "Game over", "Score: " + Game.Score, "Press Space to restart the game" })
                {
                    SizeF msgSize = canvas.MeasureString(s, Font);
                    var msgPoint = new PointF(centerWidth - msgSize.Width / 2, y += 16);
                    canvas.DrawString(s, Font, Brushes.White, msgPoint);
                }
            }
            else
            {
                var tileVec = new Vec2(TileSize - 1, TileSize - 1);
                var brush = Game.Nitro ? BrightenBrush(Colors[Game.ColorNum]) : Colors[Game.ColorNum];
                foreach (var part in Game.Snake)
                {
                    DrawRectangle(canvas, part.ScaleBy(TileSize), tileVec, brush);
                }
                int textY = 4;
                canvas.DrawString("You: " + Game.Score, this.Font, brush, new PointF(4, textY));
                foreach (var pair in Game.Opponents)
                {
                    var opponent = pair.Value;
                    brush = Game.Nitro ? BrightenBrush(Colors[opponent.ColorNum]) : Colors[opponent.ColorNum];
                    try
                    {
                        foreach (var part in opponent.Snake)
                        {
                            DrawRectangle(canvas, part.ScaleBy(TileSize), tileVec, brush);
                        }
                    }
                    catch (Exception exception)
                    { }
                    var str = string.Format("Opponent {0}: {1}", pair.Key, opponent.Score);
                    canvas.DrawString(str, this.Font, brush, new PointF(4, textY += 16));
                }
                DrawRectangle(canvas, Game.Food.ScaleBy(TileSize), tileVec, Brushes.White);
            }
        }

        private static void DrawRectangle(Graphics canvas, Vec2 topLeft, Vec2 size, Brush color)
        {
            var rect = new Rectangle(topLeft.X, topLeft.Y, size.X, size.Y);
            canvas.FillRectangle(color, rect);
        }

        private static Brush BrightenBrush(Brush brush)
        {
            var color = (brush as SolidBrush).Color;
            color = Color.FromArgb(Math.Min(color.R + 128, 255), Math.Min(color.G + 128, 255), Math.Min(color.B + 128, 255));
            return new SolidBrush(color);
        }
    }
}