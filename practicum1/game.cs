using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace Template
{
    internal class Game
    {
        public struct vec2
        {
            public float x;
            public float y;

            public vec2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public vec2 rotate(float angle, vec2 rotationOrigin)
            {
                float rotatedX = (float)(Math.Cos(angle) * (x - rotationOrigin.x) - Math.Sin(angle) * (y - rotationOrigin.y) + rotationOrigin.x);
                float rotatedY = (float)(Math.Sin(angle) * (x - rotationOrigin.x) + Math.Cos(angle) * (y - rotationOrigin.y) + rotationOrigin.y);

                return new vec2(rotatedX, rotatedY);
            }

            public static vec2 Origin
            {
                get
                {
                    return new vec2(0, 0);
                }
            }
        }

        public struct quad
        {
            public vec2 tl;
            public vec2 tr;
            public vec2 bl;
            public vec2 br;

            public quad rotate(float angle, vec2 rotationOrigin)
            {
                vec2 rotTl = tl.rotate(angle, rotationOrigin);
                vec2 rotTr = tr.rotate(angle, rotationOrigin);
                vec2 rotBr = br.rotate(angle, rotationOrigin);
                vec2 rotBl = bl.rotate(angle, rotationOrigin);

                return new quad(rotTl, rotTr, rotBr, rotBl);
            }

            public quad(vec2 tl, vec2 tr, vec2 br, vec2 bl)
            {
                this.tl = tl;
                this.tr = tr;
                this.br = br;
                this.bl = bl;
            }
        }

        // member variables
        public Surface screen;

        private float a;

        private Surface map;
        private float[,] h;
        private int VBO = 0;
        private int programID = 0;
        private int attribute_vpos = 0;
        private int attribute_vcol = 0;
        private int uniform_mview = 0;
        private int vsID, fsID;
        private int vbo_pos = 0;
        private int vbo_col = 0;
        private float[] vertexData;

        // initialize
        public void Init()
        {
            a = 0;
            map = new Surface("../../assets/heightmap.png");
            h = new float[128, 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                    h[x, y] = ((float)(map.pixels[x + y * 128] & 255)) / 256;

            vertexData = new float[127 * 127 * 2 * 3 * 3];
            ReadHeightmapToVertexData();

            VBO = GL.GenBuffer();            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.BufferData<float>(BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Length * 4), vertexData, BufferUsageHint.DynamicDraw);            GL.EnableClientState(ArrayCap.VertexArray);
            GL.VertexPointer(3, VertexPointerType.Float, 12, 0);            programID = GL.CreateProgram();
            LoadShader("../../shaders/vs.glsl",
             ShaderType.VertexShader, programID, out vsID);
            LoadShader("../../shaders/fs.glsl",
             ShaderType.FragmentShader, programID, out fsID);
            GL.LinkProgram(programID);
            attribute_vpos = GL.GetAttribLocation(programID, "vPosition");
            attribute_vcol = GL.GetAttribLocation(programID, "vColor");
            uniform_mview = GL.GetUniformLocation(programID, "M");            vbo_pos = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_pos);
            GL.BufferData<float>(BufferTarget.ArrayBuffer,
             (IntPtr)(vertexData.Length * 4),
            vertexData, BufferUsageHint.StaticDraw
             );
            GL.VertexAttribPointer(attribute_vpos, 3,
             VertexAttribPointerType.Float,
            false, 0, 0
             );            vbo_col = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo_pos);
            GL.BufferData<float>(BufferTarget.ArrayBuffer,
             (IntPtr)(vertexData.Length * 4),
            vertexData, BufferUsageHint.StaticDraw
             );
            GL.VertexAttribPointer(attribute_vcol, 3,
             VertexAttribPointerType.Float,
            false, 0, 0
             );
        }
        void LoadShader(String name, ShaderType type, int program, out int ID)
        {
            ID = GL.CreateShader(type);
            using (StreamReader sr = new StreamReader(name))
                GL.ShaderSource(ID, sr.ReadToEnd());
            GL.CompileShader(ID);
            GL.AttachShader(program, ID);
            Console.WriteLine(GL.GetShaderInfoLog(ID));
        }
        // tick: renders one frame
        public void Tick()
        {
            screen.Clear(0);

            a += 0.005f;
        }

        public void RenderGL()
        {
            Matrix4 M = Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), a);
            M *= Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), 1.9f);
            M *= Matrix4.CreateTranslation(0, 0, -1);
            M *= Matrix4.CreatePerspectiveFieldOfView(1.6f, 1.3f, .1f, 1000);
            GL.UseProgram(programID);
            GL.UniformMatrix4(uniform_mview, false, ref M);

            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vcol);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 127 * 127 * 2 * 3);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 127 * 127 * 2 * 3);
        }

        private void ReadHeightmapToVertexData()
        {
            int index = 0;



            for (int x = 1; x < h.GetLength(0); x++)
                for (int y = 1; y < h.GetLength(1); y++)
                {
                    float renderXTop = (float)x / 128 - 0.5f;
                    float renderYTop = (float)y / 128 - 0.5f;

                    float renderXBot = (float)(x - 1) / 128 - 0.5f;
                    float renderYBot = (float)(y - 1) / 128 - 0.5f;

                    ReadVertexToVertexData(renderXBot, renderYBot, h[x - 1, y - 1], index++);
                    ReadVertexToVertexData(renderXTop, renderYBot, h[x, y - 1], index++);
                    ReadVertexToVertexData(renderXBot, renderYTop, h[x - 1, y], index++);
                    ReadVertexToVertexData(renderXTop, renderYBot, h[x, y - 1], index++);
                    ReadVertexToVertexData(renderXBot, renderYTop, h[x - 1, y], index++);
                    ReadVertexToVertexData(renderXTop, renderYTop, h[x, y], index++);
                }
        }

        private void ReadVertexToVertexData(float x, float y, float z, int index)
        {
            vertexData[index * 3] = x;
            vertexData[index * 3 + 1] = y;
            vertexData[index * 3 + 2] = -z/4;
        }
    }
}