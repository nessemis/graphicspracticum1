using System;

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

            a += 0.05f;

            DrawRotatingQuad(b, a);
        }

        public void DrawRotatingQuad(quad q, float angle)
        {
            quad rq = q.rotate(angle, vec2.Origin);

            screen.Line(TX(rq.tl.x), TY(rq.tl.y), TX(rq.tr.x), TY(rq.tr.y), 0xff0000);
            screen.Line(TX(rq.tr.x), TY(rq.tr.y), TX(rq.br.x), TY(rq.br.y), 0x00ff00);
            screen.Line(TX(rq.br.x), TY(rq.br.y), TX(rq.bl.x), TY(rq.bl.y), 0x0000ff);
            screen.Line(TX(rq.bl.x), TY(rq.bl.y), TX(rq.tl.x), TY(rq.tl.y), 0xffffff);
        }

        public int TX(float x)
        {
            return (int)((x + 2.0f) * (screen.width / 4));
        }

        public int TY(float y)
        {
            return (int)(((-y * (screen.width / (float)screen.height) + 2.0f)) * (screen.height / 4));
        }


    }
}