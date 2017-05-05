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

        //angle the landscape is rotated.
        private float a;

        //memory location where all the information is in the gpu.
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

        //matrix that scales the landscape to be displayed properly
        private Matrix4 ScaleMatrix;

        //matrix that transforms the scaled landscape to the center of the screen.
        private Matrix4 TransformationMatrix;

        //data send to the gpu, contains both position and the normal vector.
        private float[] vertexData;

        // initialize
        public void Init()
        {
            a = 0;

            vertexData = HeightmapConverter.GetVertexData();

            CreateTransformationMatrix();

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

        //renders the landscape
        public void RenderGL()
        {
            Matrix4 M = TransformationMatrix;
            M *= Matrix4.CreateFromAxisAngle(new Vector3(0, 0, 1), a);
            M *= Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), 1.9f);
            M *= Matrix4.CreateTranslation(0, 0, -1);
            M *= Matrix4.CreatePerspectiveFieldOfView(1.6f, 1.3f, .1f, 1000);

            GL.UniformMatrix4(uniform_mview, false, ref M);

            GL.EnableVertexAttribArray(attribute_vpos);
            GL.EnableVertexAttribArray(attribute_vcol);
            GL.EnableVertexAttribArray(attribute_vnorm);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 127 * 127 * 2 * 3);
        }

        private void InitOpenGL()
        {
            //creates the buffer in which we store the vertexData.
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertexData.Length * 4), vertexData, BufferUsageHint.StaticDraw);

            //compiles the shader program
            programID = GL.CreateProgram();
            LoadShader("../../shaders/vs.glsl",
             ShaderType.VertexShader, programID, out vsID);
            LoadShader("../../shaders/fs.glsl",
             ShaderType.FragmentShader, programID, out fsID);
            GL.LinkProgram(programID);

            //retrieve attributes from the shader program
            attribute_vpos = GL.GetAttribLocation(programID, "vPosition");
            attribute_vnorm = GL.GetAttribLocation(programID, "vNormal");
            attribute_vcol = GL.GetAttribLocation(programID, "vColor");
            uniform_mview = GL.GetUniformLocation(programID, "M");
            uniform_sview = GL.GetUniformLocation(programID, "S");

            GL.UseProgram(programID);

            //insert the scale matrix
            GL.UniformMatrix4(uniform_sview, false, ref ScaleMatrix);

            //specify which part of the vertex data is equivalent to which attribute in the shader program
            vbo_pos = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(attribute_vpos, 3, VertexAttribPointerType.Float, false, 24, 0);

            vbo_norm = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(attribute_vnorm, 3, VertexAttribPointerType.Float, false, 24, 12);

            //color is currently not being used inside the shader to emphesize on the diffuse layer.
            vbo_col = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(attribute_vcol, 3, VertexAttribPointerType.Float, false, 24, 0);
        }

        private void CreateTransformationMatrix()
        {
            TransformationMatrix = Matrix4.CreateTranslation(-0.5f, -0.5f, 0);
        }

        private void CreateScaleMatrix()
        {
            ScaleMatrix = Matrix4.CreateScale(1 / 128f, 1 / 128f, -1 / 4f);
        }
    }

    //class used to read and process the heightmap.
    public static class HeightmapConverter
    {
        //raw data retrieved from the heightmap.
        private static float[,] h;

        //processed data about the landscape.
        private static float[] vertexData;

        public class Triangle{
            //the vectors define the coordinates of the triangles' vertexes.
            public Vector3 v1;
            public Vector3 v2;
            public Vector3 v3;

            public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }

            //calculate the normal of the surface.
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

        //returns the processed vertexdata.
        public static float[] GetVertexData()
        {
            ReadHeightmapToArray();

             return CalculateVertexData();
        }

        //reads the heightmap.
        private static void ReadHeightmapToArray()
        {
            Surface map = new Surface("../../assets/heightmap.png");
            h = new float[128, 128];
            for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
                    h[x, y] = ((float)(map.pixels[x + y * 128] & 255)) / 256;
        }

        //calculates the location and normal of each vertex.
        private static float[] CalculateVertexData()
        {
            vertexData = new float[127 * 127 * 2 * 3 * 3 * 3];

            int index = 0;

            for (int x = 1; x < h.GetLength(0) - 1; x++)
                for (int y = 1; y < h.GetLength(1) - 1; y++)
                {
                    WriteVertexToVertexData(x - 1, y - 1, h[x - 1, y - 1], index++);
                    WriteVertexToVertexData(x, y - 1, h[x, y - 1], index++);
                    WriteVertexToVertexData(x - 1, y, h[x - 1, y], index++);
                    WriteVertexToVertexData(x, y - 1, h[x, y - 1], index++);
                    WriteVertexToVertexData(x - 1, y, h[x - 1, y], index++);
                    WriteVertexToVertexData(x, y, h[x, y], index++);
                }
            return vertexData;
        }

        //writes each vertex to the vertexdata
        private static void WriteVertexToVertexData(int x, int y, float z, int index)
        {
            Vector3 normalVertex = CalculateVertexNormal(x, y);

            vertexData[index * 6] = x;
            vertexData[index * 6 + 1] = y;
            vertexData[index * 6 + 2] = z;
            vertexData[index * 6 + 3] = normalVertex.X;
            vertexData[index * 6 + 4] = normalVertex.Y;
            vertexData[index * 6 + 5] = normalVertex.Z;
        }

        //calculates the normal of the vertex at position x,y.
        private static Vector3 CalculateVertexNormal(int x, int y)
        {
            return GetAdjacentTriangleSum(x, y, GetAdjacentVertexes(x, y));
        }

        //returns the sum of the normal vectors of the adjacent triangles.
        private static Vector3 GetAdjacentTriangleSum(int x, int y, List<Vector3> adjacentVertexes)
        {
            int adjacentTriangleCount = adjacentVertexes.Count;

            if (adjacentTriangleCount == 2)
                adjacentTriangleCount = adjacentTriangleCount - 1;

            Vector3 adjacentSum = Vector3.Zero;

            Vector3 v1 = new Vector3(x, y, h[x, y]);

            for (int i = 0; i < adjacentTriangleCount; i++)
            {
                Vector3 v2 = adjacentVertexes[i];
                Vector3 v3 = adjacentVertexes[(i + 1) % adjacentVertexes.Count];

                Vector3 triangleNormal = new Triangle(v1, v2, v3).SurfaceNormal;
                if(!float.IsNaN(triangleNormal.X + triangleNormal.Y + triangleNormal.Z))
                    adjacentSum += triangleNormal;
            }

            return Vector3.Normalize(adjacentSum);
        }

        //returns a list with the adjacent vertexes.
        private static List<Vector3> GetAdjacentVertexes(int x, int y)
        {
            List<Vector3> adjacentVertexes = new List<Vector3>();
            for (int di = -1; di < 3; di++)
            {
                uint xLoc = (uint) (x + (di % 2));
                uint yLoc = (uint) (y + (di - 1) % 2);

                if (xLoc < h.GetLength(0) && yLoc < h.GetLength(1))
                {
                    adjacentVertexes.Add(new Vector3(xLoc, yLoc, h[xLoc, yLoc]));
                }
            }
            return adjacentVertexes;
        }
    }
}