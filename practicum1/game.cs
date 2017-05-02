using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

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
            GL.VertexPointer(3, VertexPointerType.Float, 12, 0);
        }

        // tick: renders one frame
        public void Tick()
        {
            screen.Clear(0);

            a += 0.005f;
        }

        public void RenderGL()
        {
            var M = Matrix4.CreatePerspectiveFieldOfView(1.6f, 1.3f, .1f, 1000);
            GL.LoadMatrix(ref M);
            GL.Translate(0, 0, -1);
            GL.Rotate(110, 1, 0, 0);
            GL.Rotate(a * 180 / Math.PI, 0, 0, 1);

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