using SharpGL;
using SharpGL.SceneGraph;
using System;
using System.Drawing;

namespace Snake
{
    public static class Util
    {
        static Random _random = new Random();
        public static int Rand() => _random.Next();
        public static int Random(int min, int max) => _random.Next(min, max);
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

        public static void MakeBuffer(OpenGL gl, GLColor color)
        {
            float[] vertices =
            {
                // back
                0,0,0, 1,0,0, 1,1,0,
                0,0,0, 0,1,0, 1,1,0,
                // front
                0,0,1, 1,0,1, 1,1,1,
                0,0,1, 0,1,1, 1,1,1,
                // left
                0,0,0, 0,0,1, 0,1,1,
                0,0,0, 0,1,0, 0,1,1,
                // right
                1,0,0, 1,0,1, 1,1,1,
                1,0,0, 1,1,0, 1,1,1,
                // bottom
                0,0,0, 1,0,0, 1,0,1,
                0,0,0, 0,0,1, 1,0,1,
                // top
                0,1,0, 1,1,0, 1,1,1,
                0,1,0, 0,1,1, 1,1,1
            };
            float[] colors =
            {
            //  r g b
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
                1,1,1, 1,1,1, 1,1,1,
            };
            float[] coef = { 0.00f, 1.00f, 0.25f, 0.25f, 0.00f, 0.50f };
            float[] clarr = { color.R, color.G, color.B };
            for (int i = 0; i < vertices.Length; ++i)
            {
                int faceNum = i / 18;
                int which = i % 3;
                colors[i] *= coef[faceNum] * clarr[which];
            }

            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 36);
            var colorBuffer = new uint[1];
            colorBuffer.Initialize();
            gl.GenBuffers(1, colorBuffer);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, colorBuffer[0]);
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, colors, OpenGL.GL_STATIC_DRAW);

            var vertexBuffer = new uint[1];
            gl.GenBuffers(1, vertexBuffer);
            gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, vertexBuffer[0]);
            gl.BufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, vertices, OpenGL.GL_STATIC_DRAW);
            ushort[] cubeVertexIndices =
            {
                  0,  1,  2,      0,  2,  3,
                  4,  5,  6,      4,  6,  7,
                  8,  9,  10,     8,  10, 11,
                  12, 13, 14,     12, 14, 15,
                  16, 17, 18,     16, 18, 19,
                  20, 21, 22,     20, 22, 23
            };

            gl.BufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, cubeVertexIndices, OpenGL.GL_STATIC_DRAW);
            gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, vertexBuffer[0]);
        }

        public static void DrawGLBoх(OpenGL gl, GLColor color, Vertex coordinates)
        {
            gl.Translate(coordinates.X, coordinates.Y, coordinates.Z);
            // setmatrixuniforms?
            gl.DrawElements(OpenGL.GL_TRIANGLES, 36, OpenGL.GL_UNSIGNED_SHORT, new IntPtr(0));
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
