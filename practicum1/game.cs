using System;
using System.IO;

namespace Template {


    class Game
    {
        

        public class vec2
        {
            public float x;
            public float y;
            public void rotate(float a)
            {
               
                 x = (float)(x * Math.Cos(a) - y * Math.Sin(a));
                 y = (float)(x * Math.Sin(a) + y * Math.Cos(a));
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

           
        }
        // member variables
        public Surface screen;
        quad b;
        float a = 0;
        // initialize
        public void Init()
        {
            b = new quad(new vec2(-0.5f, 0.5f), new vec2(0.5f, 0.5f), new vec2(0.5f, -0.5f), new vec2(-0.5f, -0.5f));
        }
        // tick: renders one frame
        public void Tick()
        {
            screen.Clear(0);
            a += 0.0005f;

            screen.Line(220, 100, 420, 100, 0x00ffff);
            screen.Line(220, 100, 220, 300, 0x00ffff);
            screen.Line(420, 100, 420, 300, 0x00ffff);
            screen.Line(220, 300, 420, 300, 0x00ffff);
            b.rotate(a);
            for (int i = 0;i<2;i++)
            {
                screen.Line(TX(b.tl.x), TY(b.tl.y), TX(b.tr.x), TY(b.tr.y), 0xff0000);
                screen.Line(TX(b.tr.x), TY(b.tr.y), TX(b.br.x), TY(b.br.y), 0x00ff00);
                screen.Line(TX(b.br.x), TY(b.br.y), TX(b.bl.x), TY(b.bl.y), 0x0000ff);
                screen.Line(TX(b.bl.x), TY(b.bl.y), TX(b.tl.x), TY(b.tl.y), 0xffffff);
            }
        }

        public int TX(float x)
        {
            return (int)((x + 1.0f) * (screen.width/4));
        }

        public int TY(float y)
        {
            return (int)((-y + 1.0f) * (screen.width*(screen.width/screen.height) /4 ));
        }


    }
} // namespace Template