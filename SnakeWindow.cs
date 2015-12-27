using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Snake
{
    public partial class SnakeWindow : Form
    {
        static Vec2 MaxWindowSize = new Vec2(700, 500);
        static int GridSize = 50;
        static SolidBrush[] Colors = new uint[] {
            0xff0000, 0x0040ff, 0x008000, 0xffff00, 0xff8000, 0x00ffff, 0xff00ff
        }.Select(i => new SolidBrush(Color.FromArgb((int)(i + 0xff000000)))).ToArray();

        int _speed;
        int Speed
        {
            get { return _speed; }
            set
            {
                Timer.Interval = 1000 / value;
                GLControl.FrameRate = value;
                _speed = value;
            }
        }

        bool _openGLMode;
        bool OpenGLMode
        {
            get { return _openGLMode; }
            set
            {
                var strs = new string[] { "Change to OpenGL", "Fall back to GDI" };
                if (value)
                {
                    Canvas.Hide();
                    GLControl.Show();
                    Timer.Stop();
                    ChangeModeButton.Text = strs[1];
                }
                else
                {
                    GLControl.Hide();
                    Canvas.Show();
                    Timer.Start();
                    ChangeModeButton.Text = strs[0];
                }
                _openGLMode = value;
            }
        }

        Game Game;
        int TileSize;
        Timer Timer = new Timer();

        public SnakeWindow()
        {
            InitializeComponent();
            TileSize = Math.Min(MaxWindowSize.X, MaxWindowSize.Y) / GridSize;
            var Grid = new Vec2(MaxWindowSize.X / TileSize, MaxWindowSize.Y / TileSize);

            Canvas.Top = MenuStrip.Height;
            Canvas.Width = Grid.X * TileSize;
            Canvas.Height = Grid.Y * TileSize;
            Width = Canvas.Width + 50;
            Height = Canvas.Height + 80;

            Canvas.Paint += Repaint;

            Game = new Game(Grid);
            Game.OnMessage += GameOnMessage;
            KeyPreview = true;
            GLControl.PreviewKeyDown += GLControlOnPreviewKeyDown;
            KeyDown += HandleKeyDown;
            KeyUp += (s, e) => Game.HandleKeyUp(e.KeyCode);
            Game.Start();

            Speed = Game.InitialSpeed;
            Timer.Tick += TimerOnTick;

            OpenGLMode = false;
        }

        private void GLControlOnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            var key = e.KeyValue;
            if (key < 37 || key > 40) return;
            HandleKeyDown(sender, new KeyEventArgs(e.KeyData));
        }

        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                OpenGLMode = !OpenGLMode;
                return;
            }
            Game.HandleKeyDown(e.KeyCode);
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            UpdateGame();
            Canvas.Invalidate();
        }

        private void UpdateGame()
        {
            if (Speed > Game.InitialSpeed)
            {
                if (!Game.Nitro)
                {
                    Speed = Game.InitialSpeed;
                }
            }
            else if (Game.Nitro)
            {
                Speed <<= 1;
            }
            Game.Update();
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
                var brush = DrawSnake(canvas, Game, tileVec);
                int textY = 4;
                canvas.DrawString("You: " + Game.Score, this.Font, brush, new PointF(4, textY));
                foreach (var pair in Game.Opponents)
                {
                    var opponent = pair.Value;
                    brush = DrawSnake(canvas, pair.Value, tileVec);
                    var str = string.Format("Opponent {0}: {1}", pair.Key, opponent.Score);
                    canvas.DrawString(str, this.Font, brush, new PointF(4, textY += 16));
                }
                DrawRectangle(canvas, Game.Food.ScaleBy(TileSize), tileVec, Brushes.White);
            }
        }

        private SolidBrush DrawSnake(Graphics canvas, Player player, Vec2 partSize)
        {
            var brush = Colors[player.ColorNum];
            if (player.Nitro)
            {
                brush = BrightenBrush(brush);
            }
            try
            {
                foreach (var part in player.Snake)
                {
                    DrawRectangle(canvas, part.ScaleBy(TileSize), partSize, brush);
                }
            }
            catch (Exception ex)
            {
                // swallow
            }
            return brush;
        }

        private static SolidBrush BrightenBrush(SolidBrush brush)
        {
            var color = brush.Color;
            color = Color.FromArgb(Math.Min(color.R + 128, 255), Math.Min(color.G + 128, 255), Math.Min(color.B + 128, 255));
            return new SolidBrush(color);
        }

        private static void DrawRectangle(Graphics canvas, Vec2 topLeft, Vec2 size, Brush brush)
        {
            var rect = new Rectangle(topLeft.X, topLeft.Y, size.X, size.Y);
            canvas.FillRectangle(brush, rect);
        }

        private void GameOnMessage(object sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = e.ToString();
            }
            catch (InvalidOperationException ex)
            {
                // swallow
            }
        }

        void OnChangeModeButtonClick(object sender, EventArgs e)
        {
            OpenGLMode = !OpenGLMode;
        }

        private void GLInitialized(object sender, EventArgs e)
        {
            OpenGL gl = GLControl.OpenGL;
            gl.ClearColor(0, 0, 0, 0);
        }

        private void GLResized(object sender, EventArgs e)
        {
            OpenGL gl = GLControl.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(100.0f, (double)Width / (double)Height, 0.01, 1000.0);
            gl.LookAt(1, 30, 30, 0, 0, 0, 0, 1, 0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        void GLDraw(object sender, RenderEventArgs e)
        {
            Game.Update();
            if (Canvas.Visible)
            {
                Canvas.Hide();
            }

            if (Game.IsOver)
            {
                Canvas.Show();
                return;
            }

            OpenGL gl = GLControl.OpenGL;
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            gl.Translate((-Game.Grid.X >> 1) - 10, -5, -20);
            gl.LineWidth(0.25f);
            gl.Color(0.1f, 0.1f, 0.1f, 1f);
            gl.Begin(OpenGL.GL_LINES);
            for (int x = 0; x <= Game.Grid.X; ++x)
            {
                gl.Vertex(x, 0, 0);
                gl.Vertex(x, 0, Game.Grid.Y);
            }
            for (int y = 0; y <= Game.Grid.Y; ++y)
            {
                gl.Vertex(0, 0, y);
                gl.Vertex(Game.Grid.X, 0, y);
            }
            gl.End();
            DrawGLSnake(gl, Game);
            foreach (var pair in Game.Opponents)
            {
                var opponent = pair.Value;
                DrawGLSnake(gl, opponent);
                //var str = string.Format("Opponent {0}: {1}", pair.Key, opponent.Score);
                //GLControl.DrawString(str, this.Font, brush, new PointF(4, textY += 16));
            }
            DrawGLBox(gl, new Vertex(Game.Food.X, 0, Game.Food.Y), new Vertex(Game.Food.X + 1, 1, Game.Food.Y + 1), new GLColor(1, 1, 1, 1));
            var borderColor = new GLColor(0.8f, 0.8f, 0.8f, 1f);
            DrawGLBox(gl, new Vertex(-1, 1, -1), new Vertex(0, 0, Game.Grid.Y), borderColor);
            DrawGLBox(gl, new Vertex(Game.Grid.X, 1, -1), new Vertex(Game.Grid.X + 1, 0, Game.Grid.Y), borderColor);
            DrawGLBox(gl, new Vertex(-1, 1, -1), new Vertex(Game.Grid.X, 0, 0), borderColor);
            DrawGLBox(gl, new Vertex(-1, 1, Game.Grid.Y), new Vertex(Game.Grid.X + 1, 0, Game.Grid.Y + 1), borderColor);

        }

        void DrawGLSnake(OpenGL gl, Player player)
        {
            var brush = Colors[Game.ColorNum];
            if (Game.Nitro)
            {
                brush = BrightenBrush(brush);
            }
            var brushColor = brush.Color;
            var color = new GLColor(brushColor.R / 255.0f, brushColor.G / 255.0f, brushColor.B / 255.0f, brushColor.A / 255.0f);
            try {
                foreach (var part in Game.Snake)
                {
                    DrawGLBox(gl, new Vertex(part.X, 0, part.Y), new Vertex(part.X + 1, 1, part.Y + 1), color);
                }
            }
            catch (Exception ex)
            {
                // swallow
            }
        }

        void DrawGLBox(OpenGL gl, Vertex a, Vertex b, GLColor color)
        {
            Vertex v1 = new Vertex(a.X, a.Y, b.Z), v2 = new Vertex(a.X, b.Y, a.Z), v3 = new Vertex(a.X, b.Y, b.Z),
                v4 = new Vertex(b.X, a.Y, a.Z), v5 = new Vertex(b.X, a.Y, b.Z), v6 = new Vertex(b.X, b.Y, a.Z);
            var triangleVertices = new Vertex[][] {
                new Vertex[] { a, v1, v3 }, new Vertex[] { a, v2, v3 }, new Vertex[] { v4, v5, b }, new Vertex[] { v4, v6, b },
                new Vertex[] { a, v1, v5 }, new Vertex[] { a, v4, v5 }, new Vertex[] { v2, v3, b }, new Vertex[] { v2, v6, b },
                new Vertex[] { v1, v2, v6 }, new Vertex[] { v1, v4, v6 }, new Vertex[] { v1, v3, b }, new Vertex[] { v1, v5, b }
            };
            gl.Begin(OpenGL.GL_TRIANGLES);
            for (int i = 0; i < triangleVertices.Length; ++i)
            {
                var vertices = triangleVertices[i];
                if (i < 4)
                {
                    gl.Color(Math.Max(color.R - 0.75f, 0), Math.Max(color.G - 0.75f, 0), Math.Max(color.B - 0.75f, 0));
                }
                else if (i < 8)
                {
                    gl.Color(Math.Max(color.R - 0.5f, 0), Math.Max(color.G - 0.5f, 0), Math.Max(color.B - 0.5f, 0));
                }
                else
                {
                    gl.Color(color);
                }
                foreach (var v in vertices)
                {
                    gl.Vertex(v);
                }
            }

            gl.End();
        }
    }
}