using SharpGL;

namespace Snake
{
    public class VBO
    {
        public uint[] VertexBuf { get; }
        public uint[] IndexBuf { get; }
        public VBO(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            //create the vertices
            float[] vertices =
            {
                // back
                -0.5f,-0.5f,-0.5f, +0.5f,-0.5f,-0.5f, +0.5f,+0.5f,-0.5f,
                -0.5f,-0.5f,-0.5f, -0.5f,+0.5f,-0.5f, +0.5f,+0.5f,-0.5f,
                // front
                -0.5f,-0.5f,+0.5f, +0.5f,-0.5f,+0.5f, +0.5f,+0.5f,+0.5f,
                -0.5f,-0.5f,+0.5f, -0.5f,+0.5f,+0.5f, +0.5f,+0.5f,+0.5f,
                // left
                -0.5f,-0.5f,-0.5f, -0.5f,-0.5f,+0.5f, -0.5f,+0.5f,+0.5f,
                -0.5f,-0.5f,-0.5f, -0.5f,+0.5f,-0.5f, -0.5f,+0.5f,+0.5f,
                // right
                +0.5f,-0.5f,-0.5f, +0.5f,-0.5f,+0.5f, +0.5f,+0.5f,+0.5f,
                +0.5f,-0.5f,-0.5f, +0.5f,+0.5f,-0.5f, +0.5f,+0.5f,+0.5f,
                // bottom
                -0.5f,-0.5f,-0.5f, +0.5f,-0.5f,-0.5f, +0.5f,-0.5f,+0.5f,
                -0.5f,-0.5f,-0.5f, -0.5f,-0.5f,+0.5f, +0.5f,-0.5f,+0.5f,
                // top
                -0.5f,+0.5f,-0.5f, +0.5f,+0.5f,-0.5f, +0.5f,+0.5f,+0.5f,
                -0.5f,+0.5f,-0.5f, -0.5f,+0.5f,+0.5f, +0.5f,+0.5f,+0.5f
            };
            VertexBuf = new uint[1];
            gl.GenBuffers(1, VertexBuf); // generate a new VBO and get the associated ID
            gl.BindBuffer(OpenGL.GL_ARRAY_BUFFER, VertexBuf[0]); // bind VBO in order to use
            gl.BufferData(OpenGL.GL_ARRAY_BUFFER, vertices, OpenGL.GL_STATIC_DRAW); // upload data to VBO

            //create the indices
            ushort[] indices = {
                  0, 1, 2,  3, 4, 5,
                  6, 7, 8,  9,10,11,
                  12,13,14,15,16,17,
                  18,19,20,21,22,23,
                  24,25,26,27,28,29,
                  30,31,32,33,34,35
            };
            IndexBuf = new uint[1];
            gl.GenBuffers(1, IndexBuf); // generate a new VBO for the indices
            gl.BindBuffer(OpenGL.GL_ELEMENT_ARRAY_BUFFER, IndexBuf[0]); // bind VBO in order to use
            gl.BufferData(OpenGL.GL_ELEMENT_ARRAY_BUFFER, indices, OpenGL.GL_STATIC_DRAW); // upload index data to VBO
        }
    }
}
