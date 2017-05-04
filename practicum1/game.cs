using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;

namespace Template
{
    internal class Game
    {
        // member variables
        public Surface screen;

        private float a;

        private int VBO = 0;
        private int programID = 0;
        private int attribute_vpos = 0;
        private int attribute_vcol = 0;
        private int attribute_vnorm = 0;
        private int uniform_mview = 0;
        private int uniform_sview = 0;
        private int vsID, fsID;
        private int vbo_pos = 0;
        private int vbo_norm = 0;
        private int vbo_col = 0;

        private Matrix4 NormalizationMatrix;

        private Matrix4 ScaleMatrix;

        private float[] vertexData;

        // initialize
        public void Init()
        {
            a = 0;

            vertexData = HeightmapConverter.GetVertexData();

            CreateNormalizationMatrix();

            CreateScaleMatrix();

            InitOpenGL();
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
            Matrix4 M = NormalizationMatrix;
            M *= Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), a);
            M *= Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), 1.9f);
            M *= Matrix4.CreateTranslation(0, 0, -1);
            M *= Matrix4.CreatePerspectiveFieldOfView(1.6f, 1.3f, .1f, 1000);

            GL.UniformMatrix4(uniform_mview, false, ref M);

            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vcol);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 127 * 127 * 2 * 3);


        }

        private void InitOpenGL()
        {
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Length * 4), vertexData, BufferUsageHint.StaticDraw);

            programID = GL.CreateProgram();
            LoadShader("../../shaders/vs.glsl",
             ShaderType.VertexShader, programID, out vsID);
            LoadShader("../../shaders/fs.glsl",
             ShaderType.FragmentShader, programID, out fsID);
            GL.LinkProgram(programID);
            attribute_vpos = GL.GetAttribLocation(programID, "vPosition");
            attribute_vnorm = GL.GetAttribLocation(programID, "vNormal");
            attribute_vcol = GL.GetAttribLocation(programID, "vColor");
            uniform_mview = GL.GetUniformLocation(programID, "M");
            uniform_sview = GL.GetUniformLocation(programID, "S");

            GL.UseProgram(programID);

            GL.UniformMatrix4(uniform_sview, false, ref ScaleMatrix);

            vbo_pos = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 24, 0);

            vbo_norm = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(attribute_vnorm, 3, VertexAttribPointerType.Float, false, 24, 12);

            vbo_col = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, false, 24, 0);
        }

        private void CreateNormalizationMatrix()
        {
            NormalizationMatrix = Matrix4.CreateTranslation(-0.5f, -0.5f, 0);
        }

        private void CreateScaleMatrix()
        {
            ScaleMatrix = Matrix4.CreateScale(1 / 128f, 1 / 128f, -1 / 4f);
        }
    }

    public static class HeightmapConverter
    {
        private static float[,] h;
        private static float[] vertexData;

        public class Triangle{
            public Vector3 v1;
            public Vector3 v2;
            public Vector3 v3;

            public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }

            public Vector3 SurfaceNormal
            {
                get
                {
                    Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);
                    normal.Normalize();
                    return normal;
                }
            }
        }

        public static float[] GetVertexData()
        {
            ReadHeightmapToArray();

             return CalculateVertexDataAndNormals();
        }

        private static void ReadHeightmapToArray()
        {
            Surface map = new Surface("../../assets/heightmap.png");
            h = new float[128, 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                    h[x, y] = ((float)(map.pixels[x + y * 128] & 255)) / 256;
        }

        private static float[] CalculateVertexDataAndNormals()
        {
            vertexData = new float[127 * 127 * 2 * 3 * 3 * 3];

            int index = 0;

            for (int x = 1; x < h.GetLength(0) - 1; x++)
                for (int y = 1; y < h.GetLength(1) - 1; y++)
                {
                    ProcessVertex(x - 1, y - 1, h[x - 1, y - 1], index++);
                    ProcessVertex(x, y - 1, h[x, y - 1], index++);
                    ProcessVertex(x - 1, y, h[x - 1, y], index++);
                    ProcessVertex(x, y - 1, h[x, y - 1], index++);
                    ProcessVertex(x - 1, y, h[x - 1, y], index++);
                    ProcessVertex(x, y, h[x, y], index++);
                }
            return vertexData;
        }

        private static void ProcessVertex(int x, int y, float z, int index)
        {
            Vector3 normalVertex = CalculateVertexNormal(x, y);

            vertexData[index * 6] = x;
            vertexData[index * 6 + 1] = y;
            vertexData[index * 6 + 2] = z;
            vertexData[index * 6 + 3] = normalVertex.X;
            vertexData[index * 6 + 4] = normalVertex.Y;
            vertexData[index * 6 + 5] = normalVertex.Z;
        }

        private static Vector3 CalculateVertexNormal(int x, int y)
        {
            return GetAdjacentTriangleSum(x, y, GetAdjacentVectors(x, y));
        }

        private static Vector3 GetAdjacentTriangleSum(int x, int y, List<Vector3> adjacentVertexes)
        {
            int adjacentTriangleCount = 4;

            if (adjacentVertexes.Count != 4)
                adjacentTriangleCount = adjacentVertexes.Count - 1;

            Vector3 adjacentSum = Vector3.Zero;

            Vector3 v1 = new Vector3(x, y, h[x, y]);

            for (int i = 0; i < adjacentTriangleCount; i++)
            {
                Vector3 v2 = adjacentVertexes[i];
                Vector3 v3 = adjacentVertexes[(i + 1) % 4];

                adjacentSum += new Triangle(v1, v2, v3).SurfaceNormal;
            }

            return Vector3.Normalize(adjacentSum);
        }

        private static List<Vector3> GetAdjacentVectors(int x, int y)
        {
            List<Vector3> adjacentVertexes = new List<Vector3>();
            for (int dx = -1; dx < 2; dx += 2)
                for (int dy = -1; dy < 2; dy += 2)
                {
                    uint xLoc = (uint)(x + dx);
                    uint yLoc = (uint)(y + dy);

                    if (xLoc < h.GetLength(0) && yLoc < h.GetLength(1))
                    {
                        adjacentVertexes.Add(new Vector3(xLoc, yLoc, h[xLoc, yLoc]));
                    }
                }
            return adjacentVertexes;
        }
    }
}