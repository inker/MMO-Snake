using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Drawing;

namespace Snake
{
    public static class Util
    {
        public static Color BrightenColor(Color color) => 
            Color.FromArgb(Math.Min(color.R + 128, 255), Math.Min(color.G + 128, 255), Math.Min(color.B + 128, 255));

        public static SolidBrush BrightenBrush(SolidBrush brush) => 
            new SolidBrush(BrightenColor(brush.Color));

        public static void DrawRectangle(Graphics canvas, Vec2 topLeft, Vec2 size, Brush brush) =>
            canvas.FillRectangle(brush, new Rectangle(topLeft.X, topLeft.Y, size.X, size.Y));

        public static GLColor DrawingColorToGLColor(Color color) =>
            new GLColor(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

        public static void DrawGLScore(OpenGL gl, int y, string playerName, int score, GLColor color) =>
            gl.DrawText(10, y, color.R, color.G, color.B, "Arial", 16, playerName + ": " + score);

        public static void AddAntialiasing(OpenGL gl)
        {
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Enable(OpenGL.GL_BLEND);
            gl.Hint(OpenGL.GL_POINT_SMOOTH_HINT, OpenGL.GL_NICEST);
            gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);
            gl.Hint(OpenGL.GL_POLYGON_SMOOTH_HINT, OpenGL.GL_NICEST);
            gl.Enable(OpenGL.GL_POINT_SMOOTH);
            gl.Enable(OpenGL.GL_LINE_SMOOTH);
            gl.Enable(OpenGL.GL_POLYGON_SMOOTH);
        }

        public static void DrawGLBox(OpenGL gl, Vertex a, Vertex b, GLColor color)
        {
            Vertex v1 = new Vertex(a.X, a.Y, b.Z), v2 = new Vertex(a.X, b.Y, a.Z), 
                v3 = new Vertex(a.X, b.Y, b.Z), v4 = new Vertex(b.X, a.Y, a.Z), 
                v5 = new Vertex(b.X, a.Y, b.Z), v6 = new Vertex(b.X, b.Y, a.Z);
            var maxZ = Math.Max(a.Z, b.Z);
            var triangleVertices = new Vertex[][] {
                new Vertex[] { a, v1, v3 }, new Vertex[] { a, v2, v3 },
                new Vertex[] { v4, v5, b }, new Vertex[] { v4, v6, b },
                new Vertex[] { a, v1, v5 }, new Vertex[] { a, v4, v5 },
                new Vertex[] { v2, v3, b }, new Vertex[] { v2, v6, b },
                new Vertex[] { v1, v2, v6 }, new Vertex[] { v1, v4, v6 },
                new Vertex[] { v1, v3, b }, new Vertex[] { v1, v5, b }
            };
            gl.Begin(OpenGL.GL_TRIANGLES);
            for (int i = 0; i < triangleVertices.Length; ++i)
            {
                var vertices = triangleVertices[i];
                var resultingColor = new float[] { color.R, color.G, color.B };
                float c = vertices[0].X == vertices[1].X && vertices[0].X == vertices[2].X
                    ? 0.25f : (vertices[0].Z != maxZ || vertices[1].Z != maxZ || vertices[2].Z != maxZ) 
                    ? 0.5f : 1f;

                for (int j = 0; j < 3; ++j)
                {
                    resultingColor[j] = Math.Max(resultingColor[j] * c, 0);
                }

                gl.Color(resultingColor);

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
    }
}
