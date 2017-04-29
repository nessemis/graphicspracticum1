using System;
using System.IO;

namespace Template {

    class Game
    {
        // member variables
        public Surface screen;
        // initialize
        public void Init()
        {
        }
        // tick: renders one frame
        public void Tick()
        {
            screen.Clear(0);

            int x = screen.width / 2 - 128;
            int y = screen.height / 2 - 128;

            for(int i = x; i < 256 + x; i++)
            {
                for (int j = y; j < 256 + y; j++)
                {
                    screen.pixels[i+ j * screen.width] = 255;

                }
            }

        }
    }
} // namespace Template