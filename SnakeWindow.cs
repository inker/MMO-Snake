using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Snake
{
    public partial class SnakeWindow : Form
    {
        static Vec2 MaxWindowSize = new Vec2(700, 500);
        static int GridSize = 50;
        static SolidBrush[] Brushes = new uint[] {
            0xff0000, 0x0040ff, 0x008000, 0xffff00, 0xff8000, 0x00ffff, 0xff00ff
        }.Select(i => new SolidBrush(Color.FromArgb((int)(i + 0xff000000)))).ToArray();

        Game Game;
        int TileSize;
        Timer Timer = new Timer();
        int InitialInterval = 1000 / Game.InitialSpeed;

        bool _openGLMode;
        bool OpenGLMode
        {
            get { return _openGLMode; }
            set
            {
                if (value)
                {
                    Canvas.Hide();
                    GLControl.Show();
                    ChangeModeButton.Text = "Fall back to GDI";
                }
                else
                {
                    GLControl.Hide();
                    Canvas.Show();
                    ChangeModeButton.Text = "Change to OpenGL";
                }
                _openGLMode = value;
            }
        }

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
            //GLControl.AutoScaleMode = AutoScaleMode.Dpi;
            //GLControl.Scale(new SizeF(2f, 2f));

            Canvas.Paint += Repaint;

            Game = new Game(Grid);
            Game.OnMessage += GameOnMessage;
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
            if (Timer.Interval < InitialInterval)
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
                    var scoreString = string.Format("Opponent {0}: {1}", pair.Key, opponent.Score);
                    canvas.DrawString(scoreString, this.Font, brush, new PointF(4, offsetY += 16));
                }
                Util.DrawRectangle(canvas, Game.Food.ScaleBy(TileSize), tileVec, System.Drawing.Brushes.White);
            }
        }

        private SolidBrush DrawSnake(Graphics canvas, Player player, Vec2 partSize)
        {
            var brush = Brushes[player.ColorNum];
            if (player.Nitro)
            {
                brush = Util.BrightenBrush(brush);
            }
            try
            {
                foreach (var part in player.Snake)
                {
                    Util.DrawRectangle(canvas, part.ScaleBy(TileSize), partSize, brush);
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
            Util.AddAntialiasing(gl);
        }

        private void GLResized(object sender, EventArgs e)
        {
            OpenGL gl = GLControl.OpenGL;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(90.0f, (double)Width / (double)Height, 0.01, 1000.0);
            gl.LookAt(1, 25, 32, 0, 0, 0, 0, 1, 0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }

        void GLDraw(object sender, RenderEventArgs e)
        {
            if (Game.IsOver)
            {
                Canvas.Show();
                return;
            }

            if (Canvas.Visible)
            {
                Canvas.Hide();
            }

            OpenGL gl = GLControl.OpenGL;
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();
            //gl.Scale(0.5f, 0.5f, 0.5f);
            var a = new SharpGL.RenderContextProviders.FBORenderContextProvider();
            float tx = (-Game.Grid.X >> 1) + 5 - (float)Game.Snake[0].X / 5,
                ty = -((float)Game.Snake[0].Y / 15),
                tz = -20 - ((float)Game.Snake[0].Y / 15);
            gl.Translate(tx, ty, tz);
            
            gl.LineWidth(1f);
            gl.Color(0.1f, 0.1f, 0.1f, 1f);

            gl.Begin(OpenGL.GL_LINES);
            for (int i = 0; i <= Game.Grid.X; ++i)
            {
                gl.Vertex(i, 0, 0);
                gl.Vertex(i, 0, Game.Grid.Y);
            }
            for (int i = 0; i <= Game.Grid.Y; ++i)
            {
                gl.Vertex(0, 0, i);
                gl.Vertex(Game.Grid.X, 0, i);
            }
            gl.End();
            var color = Util.DrawGLSnake(gl, Game, Brushes);
            int offsetY = GLControl.Height - 25;
            Util.DrawGLScore(gl, offsetY -= 15, "You", Game.Score, color);
            foreach (var pair in Game.Opponents)
            {
                var opponent = pair.Value;
                color = Util.DrawGLSnake(gl, opponent, Brushes);
                Util.DrawGLScore(gl, offsetY -= 15, "Opponent " + pair.Key, opponent.Score, color);
            }
            Util.DrawGLBox(gl, new Vertex(Game.Food.X, 0, Game.Food.Y), new Vertex(Game.Food.X + 1, 1, Game.Food.Y + 1), new GLColor(1, 1, 1, 1));
            var brinkColor = new GLColor(0.8f, 0.8f, 0.8f, 1f);
            Util.DrawGLBox(gl, new Vertex(-1, 1, -1), new Vertex(0, 0, Game.Grid.Y), brinkColor);
            Util.DrawGLBox(gl, new Vertex(Game.Grid.X, 1, -1), new Vertex(Game.Grid.X + 1, 0, Game.Grid.Y), brinkColor);
            Util.DrawGLBox(gl, new Vertex(-1, 1, -1), new Vertex(Game.Grid.X, 0, 0), brinkColor);
            Util.DrawGLBox(gl, new Vertex(-1, 1, Game.Grid.Y), new Vertex(Game.Grid.X + 1, 0, Game.Grid.Y + 1), brinkColor);
        }
    }
}