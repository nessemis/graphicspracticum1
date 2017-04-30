using System;
using System.IO;

namespace Template {


    class Game
    {
        
        public static int width;
        public static  int height;

        public class vec2
        {
            public float x;
            public float y;
            public void rotate(float a)
            {
                float tempx = x;
                 x = (float)(x * Math.Cos(a) - y * Math.Sin(a) );
                 y = (float)(tempx * Math.Sin(a) + y * Math.Cos(a) );
            }
            public vec2(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
        }
       
        public class quad {
            public vec2 tl;
            public vec2 tr;
            public vec2 bl;
            public vec2 br;



          public void rotate(float a)
            {
                tl.rotate(a);
                tr.rotate(a);
                bl.rotate(a);
                br.rotate(a);
            }

            public  quad(vec2 tl, vec2 tr,vec2 br, vec2 bl)
            {
                this.tl = tl;
                this.tr = tr;
                this.br = br;
                this.bl = bl;
            }
            public void draw(Surface screen)
            {
                screen.Line(TX(tl.x), TY(tl.y), TX(tr.x), TY(tr.y), 0xff0000);
                screen.Line(TX(tr.x), TY(tr.y), TX(br.x), TY(br.y), 0x00ff00);
                screen.Line(TX(br.x), TY(br.y), TX(bl.x), TY(bl.y), 0x0000ff);
                screen.Line(TX(bl.x), TY(bl.y), TX(tl.x), TY(tl.y), 0xffffff);
            }

            public int TX(float x)
            {
                return (int)((x + 1.0f) * (Game.width / 4));
            }

            public int TY(float y)
            {
                return (int)((-y + 1.0f) * (Game.width * (Game.width / Game.height) / 4));
            }

        }
        // member variables
        public Surface screen;
        quad[] b;
        float a = 0;
        // initialize
        public void Init()
        {
            Game.width = screen.width;
            Game.height = screen.height;
            Random rnd = new Random();
            b = new quad[5];
            for(int i =0;i<5;i++ )

            b[i] = new quad(new vec2((float)-rnd.NextDouble(), (float)rnd.NextDouble()), new vec2((float)rnd.NextDouble(), (float)rnd.NextDouble()), 
                new vec2((float)rnd.NextDouble(), -(float)rnd.NextDouble()), new vec2(-(float)rnd.NextDouble(), -(float)rnd.NextDouble()));

           
        }
        // tick: renders one frame
        public void Tick()
        {
         
            screen.Clear(0);
            a += 0.005f;

            screen.Line(220, 100, 420, 100, 0x00ffff);
            screen.Line(220, 100, 220, 300, 0x00ffff);
            screen.Line(420, 100, 420, 300, 0x00ffff);
            screen.Line(220, 300, 420, 300, 0x00ffff);
           
            for (int i = 0;i<5;i++)
            {
                b[i].rotate(a);
                b[i].draw(screen);
            }
        }

    


    }
} // namespace Template