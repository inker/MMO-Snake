using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Drawing;

namespace Snake
{
    public static class Util
    {
        public static Color BrightenColor(Color color) => Color.FromArgb(Math.Min(color.R + 128, 255), Math.Min(color.G + 128, 255), Math.Min(color.B + 128, 255));
        public static SolidBrush BrightenBrush(SolidBrush brush) => new SolidBrush(BrightenColor(brush.Color));


        public static void DrawRectangle(Graphics canvas, Vec2 topLeft, Vec2 size, Brush brush)
        {
            var rect = new Rectangle(topLeft.X, topLeft.Y, size.X, size.Y);
            canvas.FillRectangle(brush, rect);
        }

        public static GLColor DrawingColorToGLColor(Color color) => new GLColor(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

        public static void DrawGLBox(OpenGL gl, Vertex a, Vertex b, GLColor color)
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

        public static GLColor DrawGLSnake(OpenGL gl, Player player, SolidBrush[] brushes)
        {
            var brushColor = brushes[player.ColorNum].Color;
            if (player.Nitro)
            {
                brushColor = BrightenColor(brushColor);
            }
            var color = DrawingColorToGLColor(brushColor);
            try
            {
                foreach (var part in player.Snake)
                {
                    DrawGLBox(gl, new Vertex(part.X, 0, part.Y), new Vertex(part.X + 1, 1, part.Y + 1), color);
                }
            }
            catch (Exception ex)
            {
                // swallow
            }
            return color;
        }

        public static void DrawGLScore(OpenGL gl, int y, string playerName, int score,GLColor color)
        {
            gl.DrawText(20, y, color.R, color.G, color.B, "Arial", 11, playerName + ": " + score);
        }

    }
}
