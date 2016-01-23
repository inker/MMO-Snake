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
        static string[] OpponentNames = new string[] 
        {
            "Foo", "Bar", "Baz", "Qux", "Quux", "Corge", "Grault", "Garply", "Waldo", "Fred", "Plugh", "Xyzzy", "Thud"
        };
        static Vec2 MaxWindowSize = new Vec2(900, 500);

        Gameplay Game;
        int TileSize;
        Timer Timer = new Timer();
        int InitialInterval;

        VBO Vbo;
        Dictionary<Color, CBO> Cbo = new Dictionary<Color, CBO>();

        bool _openGLMode;
        bool OpenGLMode
        {
            get { return _openGLMode; }
            set
            {
                if (value)
                {
                    this.TopMost = true;
                    //this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Maximized;
                    Canvas.Hide();
                    GLControl.Show();
                    ChangeModeButton.Text = "Fall back to GDI";
                }
                else
                {
                    GLControl.Hide();
                    this.TopMost = false;
                    //this.FormBorderStyle = FormBorderStyle.None;
                    this.WindowState = FormWindowState.Normal;
                    Canvas.Show();
                    ChangeModeButton.Text = "Change to OpenGL";
                }
                _openGLMode = value;
            }
        }

        public SnakeWindow()
        {
            InitializeComponent();
            var gl = GLControl.OpenGL;
            Vbo = new VBO(gl);
            Cbo[Color.White] = new CBO(gl, Color.White);
            Cbo[Color.Gray] = new CBO(gl, Color.Gray);
            //GLControl.AutoScaleMode = AutoScaleMode.Dpi;
            //GLControl.Scale(new SizeF(2f, 2f));

            Game = new Gameplay();
            InitialInterval = 1000 / Game.InitialSpeed;
            Game.OnMessage += GameOnMessage;
            Game.GridResize += (s, e) => ResizeCanvas();
            Game.SpeedChange += (s, e) => InitialInterval = 1000 / Game.InitialSpeed;

            ResizeCanvas();

            Canvas.Paint += Repaint;

            KeyPreview = true;
            GLControl.PreviewKeyDown += GLControlOnPreviewKeyDown;
            KeyDown += HandleKeyDown;
            KeyUp += (s, e) => Game.HandleKeyUp(e.KeyCode);
            Game.Start();

            Timer.Interval = InitialInterval;
            Timer.Tick += TimerOnTick;
            Timer.Start();

            OpenGLMode = false;
        }

        private void ResizeCanvas()
        {
            TileSize = Math.Min(MaxWindowSize.X / Game.Grid.X, MaxWindowSize.Y / Game.Grid.Y);
            Canvas.Top = MenuStrip.Height;
            try
            {
                var grid = Game.Grid;
                Canvas.Width = grid.X * TileSize;
                Canvas.Height = grid.Y * TileSize;
                Width = Canvas.Width + 50;
                Height = Canvas.Height + 80;
            }
            catch (Exception e) { }
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
            if (!OpenGLMode)
            {
                Canvas.Invalidate();
            }
        }

        private void UpdateGame()
        {
            if (Timer.Interval != InitialInterval)
            {
                if (!Game.Nitro)
                {
                    Timer.Interval = InitialInterval;
                }
            }
            else if (Game.Nitro)
            {
                Timer.Interval >>= 1;
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
                foreach (var s in new string[] { "Game over", "Score: " + Game.Score, "Press Enter to restart the game" })
                {
                    SizeF msgSize = canvas.MeasureString(s, Font);
                    var msgPoint = new PointF(centerWidth - msgSize.Width / 2, y += 16);
                    canvas.DrawString(s, base.Font, System.Drawing.Brushes.White, msgPoint);
                }
            }
            else
            {
                var tileVec = new Vec2(TileSize - 1, TileSize - 1);
                var brush = DrawSnake(canvas, Game, tileVec);
                int offsetY = 4;
                canvas.DrawString("You: " + Game.Score, this.Font, brush, new PointF(4, offsetY));
                foreach (var pair in Game.Opponents)
                {
                    var opponent = pair.Value;
                    brush = DrawSnake(canvas, pair.Value, tileVec);
                    var name = pair.Key < OpponentNames.Length ? OpponentNames[pair.Key] : "Opponent " + (pair.Key - OpponentNames.Length - 1);
                    var scoreString = name + ": " + opponent.Score;
                    canvas.DrawString(scoreString, this.Font, brush, new PointF(4, offsetY += 16));
                }
                Util.DrawRectangle(canvas, Game.Food * TileSize, tileVec, System.Drawing.Brushes.White);
            }
        }

        private SolidBrush DrawSnake(Graphics canvas, Player player, Vec2 partSize)
        {
            var color = player.Color;
            if (player.Nitro)
            {
                color = Util.BrightenColor(color);
            }
            var brush = new SolidBrush(color);
            try
            {
                foreach (var part in player.Snake)
                {
                    Util.DrawRectangle(canvas, part * TileSize, partSize, brush);
                }
            }
            catch (Exception ex)
            {
                // swallow
            }
            return brush;
        }

        private void GameOnMessage(object sender, EventArgs e)
        {
            try
            {
                StatusLabel.Text = (e as MessageEventArgs).Message;
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
            Util.AddAntialiasing(gl);
        }

        private void GLResized(object sender, EventArgs e)
        {
            OpenGL gl = GLControl.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(90.0f, (double)Width / (double)Height, 0.01, 1000.0);
            float y = Game != null && Game.Grid != null ? Game.Grid.Y : 150;
            gl.LookAt(1, 25 + (y - 50) * 0.5, 32 + (y - 50) * 1.2, 0, 0, 0, 0, 1, 0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);   
        }

        void GLDraw(object sender, RenderEventArgs e)
        {
            var gl = GLControl.OpenGL;
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity(); // somehow this shit ends endless movement of the scene

            if (Game.IsOver)
            {
                gl.ClearColor(0, 0, 0, 0);
                gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
                var y = 100;
                foreach (var s in new string[] { "Game over", "Score: " + Game.Score, "Press Enter to restart the game" })
                {
                    gl.DrawText(GLControl.Width / 2 - 50 - s.Length * 7, GLControl.Height - (y += 50), 1, 1, 1, "Arial", 30, s);
                }
                return;
            }

            float tx = (-Game.Grid.X >> 1) + 5 - (float)Game.Snake[0].X / 5,
                ty = -((float)Game.Snake[0].Y / 15),
                tz = -20 - ((float)Game.Snake[0].Y / 15);
            gl.Translate(tx, ty, tz);

            Util.DrawLines(gl, Game.Grid);
            gl.Translate(0.5f, 0.5f, 0.5f);

            //gl.ClearColor(0, 0, 0, 0);
            
            DrawGLSnake(gl, Game);
            int offsetY = GLControl.Height - 30;
            Util.DrawGLScore(gl, offsetY -= 25, "You", Game.Score, Game.Color);
            foreach (var pair in Game.Opponents)
            {
                var opponent = pair.Value;
                DrawGLSnake(gl, opponent);
                var name = pair.Key < OpponentNames.Length ? OpponentNames[pair.Key] : "Opponent " + (pair.Key - OpponentNames.Length - 1);
                Util.DrawGLScore(gl, offsetY -= 25, name, opponent.Score, opponent.Color);
            }
            Util.DrawGLCube(gl, Vbo, Cbo[Color.White], new Vertex(Game.Food.X, 0, Game.Food.Y));
            //var brinkColor = new GLColor(0.8f, 0.8f, 0.8f, 1f);
            ////Util.DrawGLBox(gl, new Vertex(-1, 1, -1), new Vertex(0, 0, Game.Grid.Y), brinkColor);
            ////Util.DrawGLBox(gl, new Vertex(Game.Grid.X, 1, -1), new Vertex(Game.Grid.X + 1, 0, Game.Grid.Y), brinkColor);
            ////Util.DrawGLBox(gl, new Vertex(-1, 1, -1), new Vertex(Game.Grid.X, 0, 0), brinkColor);
            ////Util.DrawGLBox(gl, new Vertex(-1, 1, Game.Grid.Y), new Vertex(Game.Grid.X + 1, 0, Game.Grid.Y + 1), brinkColor);
        }

        void DrawGLSnake(OpenGL gl, Player player)
        {

            var color = player.Color;
            if (player.Nitro)
            {
                color = Util.BrightenColor(color);
            }
            CBO cbo;
            if (!Cbo.TryGetValue(color, out cbo))
            {
                cbo = new CBO(gl, color);
                Cbo[color] = cbo;
            }
            try
            {
                foreach (var part in player.Snake)
                {
                    Util.DrawGLCube(gl, Vbo, cbo, new Vertex(part.X, 0, part.Y));
                }
            }
            catch (Exception ex)
            {
                // swallow
            }
        }
    }
}