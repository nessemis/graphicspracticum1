using System;
using OpenTK.Input;

namespace Template
{
    class Game
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

        public vec2 camera;

        public float xDiameter;

        private float xRadius
        {
            get
            {
                return xDiameter / 2;
            }
        }

        public float yDiameter
        {
            get
            {
                return screen.height / (float)screen.width * xDiameter;
            }
        }

        private float yRadius
        {
            get
            {
                return yDiameter / 2;
            }
        }

        private float xMin
        {
            get
            {
                return -camera.x - xRadius;
            }
        }

        private float xMax
        {
            get
            {
                return -camera.x + xRadius;
            }
        }

        private float yMin
        {
            get
            {
                return -camera.y - yRadius;
            }
        }

        private float yMax
        {
            get
            {
                return -camera.y + yRadius;
            }
        }

        // initialize
        public void Init()
        {
            camera = vec2.Origin;

            xDiameter = 4;
        }
        // tick: renders one frame
        public void Tick()
        {
            screen.Clear(0);

            DrawAxes();

            PrintLabels();

            PlotGraph();
        }

        public void Input(KeyboardState keyboard)
        {

            if (keyboard[OpenTK.Input.Key.Z])
            {
                xDiameter *= 1.1f;
            }
            if (keyboard[OpenTK.Input.Key.X])
            {
                xDiameter *= 0.9f;
            }

            if (keyboard[OpenTK.Input.Key.Up])
            {
                camera.y += 0.01f * xDiameter;
            }
            if (keyboard[OpenTK.Input.Key.Down])
            {
                camera.y -= 0.01f * xDiameter;
            }
            if (keyboard[OpenTK.Input.Key.Left])
            {
                camera.x += 0.01f * xDiameter;
            }
            if (keyboard[OpenTK.Input.Key.Right])
            {
                camera.x -= 0.01f * xDiameter;
            }
        }

        public void PlotGraph()
        {
            for (int i = 0; i < screen.width; i += 1)
            {
                DrawPixel(i, TY(Function(i / (float)screen.width * xDiameter + xMin)));
            }
        }

        private void DrawPixel(int x, int y)
        {
            if (x > 0 && x < screen.width && y > 0 && y < screen.height)
                screen.pixels[x + y * screen.width] = 0xFFFFFF;
        }

        public void DrawAxes()
        {
            screen.Line(0, TY(0), screen.width, TY(0), 0xF4A460);
            screen.Line(TX(0), 0, TX(0), screen.height, 0xF4A460);
        }

        public void PrintLabels()
        {
            screen.Print("" + xMin, 20, 0, 0xD3D3D);
            screen.Print("" + camera.x, screen.width / 2, 0, 0xD3D3D);

            screen.Print("" + yMin, 0, 20, 0xD3D3D);
            screen.Print("" + camera.y, 0, screen.height / 2, 0xD3D3D);
        }

        public float Function(float x)
        {
            return (float) Math.Log(x) / 8;
        }

        public int TX(float x)
        {
            return (int)(((x - xMin) / xDiameter) * screen.width);
        }

        public int TY(float y)
        {
            return (int) Math.Round(((-y - yMin) / yDiameter) * screen.height);
        }
    }
}