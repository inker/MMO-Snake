using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Drawing;

namespace Snake
{
    public static class Util
    {
        static Random _random = new Random();
        public static int Random() => _random.Next();
        public static int Random(int min, int max) => _random.Next(min, max);

        public static Color BrightenColor(Color color) => 
            Color.FromArgb(Math.Min(color.R + 128, 255), Math.Min(color.G + 128, 255), Math.Min(color.B + 128, 255));

        static Rectangle MakeRectangle(Vec2 topLeft, Vec2 size) => new Rectangle(topLeft.X, topLeft.Y, size.X, size.Y);
        public static void DrawRectangle(Graphics canvas, Vec2 topLeft, Vec2 size, Brush brush) =>
            canvas.FillRectangle(brush, MakeRectangle(topLeft, size));

        public static GLColor DrawingColorToGLColor(Color color) =>
            new GLColor(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);

        public static void DrawGLScore(OpenGL gl, int y, string playerName, int score, GLColor color) =>
            gl.DrawText(10, y, color.R, color.G, color.B, "Arial", 16, playerName + ": " + score);

        public static void DrawLines(OpenGL gl, Vec2 grid)
        {
            gl.LineWidth(1f);
            gl.Color(0.1f, 0.1f, 0.1f, 1f);
            gl.Begin(OpenGL.GL_LINES);
            for (int i = 0; i <= grid.X; ++i)
            {
                gl.Vertex(i, 0, 0);
                gl.Vertex(i, 0, grid.Y);
            }
            for (int i = 0; i <= grid.Y; ++i)
            {
                gl.Vertex(0, 0, i);
                gl.Vertex(grid.X, 0, i);
            }
            gl.End();
        }

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

        public static void foo(OpenGL gl, VBO vbo, CBO colorBuf, Vertex coordinates)
        {
            gl.Translate(coordinates.X, coordinates.Y, coordinates.Z);
            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);                   // activate vertex array
            gl.EnableClientState(OpenGL.GL_COLOR_ARRAY);                   // activate vertex array

            // bind VBOs for vertex array
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, vbo.VertexBuf[0]);      // for vertex coordinates
            gl.VertexPointer(3, OpenGL.GL_FLOAT, 0, new IntPtr(0));         // last param is offset, not ptr

            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, colorBuf.ColorBuf[0]);
            gl.ColorPointer(3, OpenGL.GL_FLOAT, 0, new IntPtr(0));

            gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, vbo.IndexBuf[0]);

            // draw 1 quads using offset of index array
            gl.DrawElements(OpenGL.GL_TRIANGLES, 108, OpenGL.GL_UNSIGNED_SHORT, new IntPtr(0));

            // bind with 0, so, switch back to normal pointer operation
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, 0);
            gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, 0);

            gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);                // deactivate vertex array
            gl.DisableClientState(OpenGL.GL_COLOR_ARRAY);                   // activate vertex array
            gl.Translate(-coordinates.X, -coordinates.Y, -coordinates.Z);
        }

        public static GLColor DrawGLSnake(OpenGL gl, Player player, VBO vbo, CBO cbo)
        {
            var brushColor = player.Color;
            if (player.Nitro)
            {
                brushColor = BrightenColor(brushColor);
            }
            var color = DrawingColorToGLColor(brushColor);
            try
            {
                foreach (var part in player.Snake)
                {
                    foo(gl, vbo, cbo, new Vertex(part.X, 0, part.Y));
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
