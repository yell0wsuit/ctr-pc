using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class Sock : CTRGameObject
    {
        public static Sock Sock_create(CTRTexture2D t)
        {
            return (Sock)new Sock().InitWithTexture(t);
        }

        public static Sock Sock_createWithResID(int r)
        {
            return Sock_create(Application.GetTexture(r));
        }

        public static Sock Sock_createWithResIDQuad(int r, int q)
        {
            Sock sock = Sock_create(Application.GetTexture(r));
            sock.SetDrawQuad(q);
            return sock;
        }

        public void CreateAnimations()
        {
            light = Animation_createWithResID(85);
            light.anchor = 34;
            light.parentAnchor = 10;
            light.y = 270f;
            light.x = RTD(0.0);
            light.AddAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 2, [3, 4, 4]);
            light.DoRestoreCutTransparency();
            light.visible = false;
            _ = AddChild(light);
        }

        public void UpdateRotation()
        {
            float num = 140f;
            t1.x = x - (num / 2f) - 20f;
            t2.x = x + (num / 2f) - 20f;
            t1.y = t2.y = y;
            b1.x = t1.x;
            b2.x = t2.x;
            b1.y = b2.y = y + 15f;
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = VectRotateAround(t1, angle, x, y);
            t2 = VectRotateAround(t2, angle, x, y);
            b1 = VectRotateAround(b1, angle, x, y);
            b2 = VectRotateAround(b2, angle, x, y);
        }

        public override void Draw()
        {
            Timeline timeline = light.GetCurrentTimeline();
            if (timeline != null && timeline.state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                light.visible = false;
            }
            base.Draw();
        }

        public override void DrawBB()
        {
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (mover != null)
            {
                UpdateRotation();
            }
        }

        public const float SOCK_IDLE_TIMOUT = 0.8f;

        public static int SOCK_RECEIVING;

        public static int SOCK_THROWING = 1;

        public static int SOCK_IDLE = 2;

        public int group;

        public double angle;

        public Vector t1;

        public Vector t2;

        public Vector b1;

        public Vector b2;

        public float idleTimeout;

        public Animation light;
    }
}
