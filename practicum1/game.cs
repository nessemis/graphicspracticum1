using System;
using OpenTK.Input;

namespace Template
{
    class Game
    {
        public struct Point
        {
            public float x;
            public float y;
            public Point(float x, float y)
            {
                this.x = x;
                this.y = y;
            }

            public Point rotate(float angle, Point rotationOrigin)
            {
                float rotatedX = (float)(Math.Cos(angle) * (x - rotationOrigin.x) - Math.Sin(angle) * (y - rotationOrigin.y) + rotationOrigin.x);
                float rotatedY = (float)(Math.Sin(angle) * (x - rotationOrigin.x) + Math.Cos(angle) * (y - rotationOrigin.y) + rotationOrigin.y);

                return new Point(rotatedX, rotatedY);
            }

            public static Point Origin
            {
                get
                {
                    return new Point(0, 0);
                }
            }
        }

        //collection of four points.
        //not used for graphing.
        public struct Quad
        {
            //topleft
            public Point tl;
            //topright
            public Point tr;
            //botleft
            public Point bl;
            //botright
            public Point br;

            //returns this quad rotated around the rotationOrigin. Doesn't rotate the quad itself
            public Quad rotate(float angle, Point rotationOrigin)
            {
                Point rotTl = tl.rotate(angle, rotationOrigin);
                Point rotTr = tr.rotate(angle, rotationOrigin);
                Point rotBr = br.rotate(angle, rotationOrigin);
                Point rotBl = bl.rotate(angle, rotationOrigin);

                return new Quad(rotTl, rotTr, rotBr, rotBl);
            }

            public Quad(Point tl, Point tr, Point br, Point bl)
            {
                this.tl = tl;
                this.tr = tr;
                this.br = br;
                this.bl = bl;
            }
        }

        //Method to plot the graph.
        //PIXEL plots the graph pixel per pixel
        //LINE connects pixels next to each other to for a line.
        public enum DrawMethod
        {
            PIXEL, LINE
        }

        // member variables
        public Surface screen;

        //Location at which the center of the camera is pointing.
        public Point camera;

        //Diameter around the x-coördinate of the camera which should be rendered to the screen.
        public float xDiameter;

        //Amount of y-values calculated per pixel.
        private float samplesPerPixel = 1;

        private DrawMethod drawMethod = DrawMethod.LINE;

        //Radius around the x-coördinate of the camera which should be rendered to the screen.
        private float xRadius
        {
            get
            {
                return xDiameter / 2;
            }
        }

        //Diameter around the y-coördinate of the camera which should be rendered to the screen.
        public float yDiameter
        {
            get
            {
                return screen.height / (float)screen.width * xDiameter;
            }
        }

        //Radius around the y-coördinate of the camera which should be rendered to the screen.
        private float yRadius
        {
            get
            {
                return yDiameter / 2;
            }
        }

        //Minimum x-value visible on the screen.
        private float xMin
        {
            get
            {
                return -camera.x - xRadius;
            }
        }

        //Minimum x-value which should plotted. It is calculated in a way to prevent different x-values being calulated for the same pixel-location.
        private float evaluateMin
        {
            get
            {
                return (float)(Math.Floor(xMin / stepSize) * stepSize);
            }
        }

        //Maximum x-value visible on the screen.
        private float xMax
        {
            get
            {
                return -camera.x + xRadius;
            }
        }

        //Minimum y-value visible on the screen.
        private float yMin
        {
            get
            {
                return -camera.y - yRadius;
            }
        }

        //Maximum y-value visible on the screen.
        private float yMax
        {
            get
            {
                return -camera.y + yRadius;
            }
        }

        //Distance between each x-value that is checked while plotting.
        private float stepSize
        {
            get
            {
                return (samplesPerPixel * xDiameter) / screen.width;
            }
        }

        // initialize
        public void Init()
        {
            camera = Point.Origin;

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
                if(xDiameter * 1.1f < (100000))
                    xDiameter *= 1.1f;
            }
            if (keyboard[OpenTK.Input.Key.X])
            {
                //we can't go smaller because of the floating-point precision imposed by the TX and TY functions.
                if(xDiameter * 0.9f > (0.01f))
                    xDiameter *= 0.9f;
            }
            if (keyboard[OpenTK.Input.Key.Number1])
            {
                drawMethod = DrawMethod.PIXEL;
            }
            if (keyboard[OpenTK.Input.Key.Number2])
            {
                drawMethod = DrawMethod.LINE;
            }
            if (keyboard[OpenTK.Input.Key.Q])
            {
                samplesPerPixel *= 1.01f;
            }
            if (keyboard[OpenTK.Input.Key.W])
            {
                samplesPerPixel *= 0.99f;
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

        //plots the graph to the screen
        public void PlotGraph()
        {
            switch(drawMethod)
            {
                case DrawMethod.LINE:
                    Point previousPoint = new Point(evaluateMin, Function(evaluateMin));
                    for (double x = evaluateMin + stepSize; x < xMax; x += stepSize)
                    {
                        Point newPoint = new Point((float)x, Function((float)x));
                        DrawLine(previousPoint, newPoint);
                        previousPoint = newPoint;
                    }
                    break;
                case DrawMethod.PIXEL:
                    for (double x = evaluateMin; x < xMax; x += stepSize)
                    {
                        DrawPixel(TX((float)x), TY(Function((float)x)));
                    }
                    break;
            }
        }

        //draws a white pixel to the screen at (x, y)
        private void DrawPixel(int x, int y)
        {
            if (x > 0 && x < screen.width && y > 0 && y < screen.height)
                screen.pixels[x + y * screen.width] = 0xFFFFFF;
        }

        //draws a white line to the screen at between v1 and v2
        private void DrawLine(Point v1, Point v2)
        {
            //points at infinity are not drawn by the screen.Line method, and are therefor not present in the graph.
            screen.Line(TX(v1.x), TY(v1.y), TX(v2.x), TY(v2.y), 0xFFFFFF);
        }

        //draws the axes of the coördinate system
        public void DrawAxes()
        {
            screen.Line(0, TY(0), screen.width, TY(0), 0xF4A460);
            screen.Line(TX(0), 0, TX(0), screen.height, 0xF4A460);
        }

        //prints labels indicating the x and y of the world coördinate system
        public void PrintLabels()
        {
            screen.Print("" + xMin, 20, 0, 0xD3D3D);
            screen.Print("" + camera.x, screen.width / 2, 0, 0xD3D3D);

            screen.Print("" + yMin, 0, 20, 0xD3D3D);
            screen.Print("" + camera.y, 0, screen.height / 2, 0xD3D3D);
        }
        
        //function to be plotted. Returns the y value for the x-value that is inserted.
        public float Function(float x)
        {
            return (float) Math.Sin(x);
        }

        //translates the world x-coördinate to the screen x-coördinate
        public int TX(float x)
        {
            return (int)(((x - xMin) / xDiameter) * screen.width);
        }

        //translates the world y-coördinate to the screen y-coördinate
        public int TY(float y)
        {
            return (int) Math.Round(((-y - yMin) / yDiameter) * screen.height);
        }
    }
}