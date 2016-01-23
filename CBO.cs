using SharpGL;
using SharpGL.SceneGraph;

namespace Snake
{
    public class CBO
    {
        static float[] Coef = { 0.00f, 1.00f, 0.25f, 0.25f, 0.00f, 0.50f };

        public uint[] ColorBuf { get; }

        public CBO(OpenGL gl, GLColor color)
        {
            float[] colors = new float[108];
            float[] colorArr = { color.R, color.G, color.B };
            for (int i = 0; i < colors.Length; ++i)
            {
                colors[i] = Coef[i / 18] * colorArr[i % 3];
            }

            ColorBuf = new uint[1];
            //color_buf.Initialize();
            gl.GenBuffers(1, ColorBuf);
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, ColorBuf[0]);
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, colors, OpenGL.GL_STATIC_DRAW);
        }
    }
}
