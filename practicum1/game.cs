using System;
using System.IO;

namespace Template {

    class Game
    {
        private float time = 0f;

        // member variables
        public Surface screen;
        // initialize
        public void Init()
        {
        }
        // tick: renders one frame
        public void Tick()
        {
            time += 0.0005f;

            screen.Clear(0);

            int x = screen.width / 2 - 128;
            int y = screen.height / 2 - 128;

            for(int i = x; i < 256 + x; i++)
            {
                int c = i - x;
                for (int j = y; j < 256 + y; j++)
                {
                    int c2 = j - y;
                    screen.pixels[RotatedScreenCoords(time, i, j)] = (c << 16 )+ (c2<<8) ;
                }
            }
        }

        private int RotatedScreenCoords(float a, int x, int y)
        {
            int rotCenterX = screen.width / 2;
            int rotCenterY = screen.height / 2;

            int rotX = TX(a, x - rotCenterX, y - rotCenterY) + rotCenterX;
            int rotY = TY(a, x - rotCenterX, y - rotCenterY) + rotCenterY;

            return rotX + rotY * screen.width;
        }

        //transforms coordinates to a rotated version
        private int TX (float a, int x, int y)
        {
            return (int) (Math.Cos(a) * x - Math.Sin(a) * y);
        }

        //transforms coordinates to a rotated version
        private int TY(float a, int x, int y)
        {
            return (int) (Math.Sin(a) * x + Math.Cos(a) * y);
        }
    }
} // namespace Template