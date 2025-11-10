using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using System;
using System.Collections.Generic;

namespace CutTheRope.game
{
    internal class Sock : CTRGameObject
    {
        public static Sock Sock_create(CTRTexture2D t)
        {
            return (Sock)new Sock().initWithTexture(t);
        }

        public static Sock Sock_createWithResID(int r)
        {
            return Sock_create(Application.getTexture(r));
        }

        public static Sock Sock_createWithResIDQuad(int r, int q)
        {
            Sock sock = Sock_create(Application.getTexture(r));
            sock.setDrawQuad(q);
            return sock;
        }

        public virtual void createAnimations()
        {
            light = Animation_createWithResID(85);
            light.anchor = 34;
            light.parentAnchor = 10;
            light.y = 270f;
            light.x = RTD(0.0);
            light.addAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 2, new List<int> { 3, 4, 4 });
            light.doRestoreCutTransparency();
            light.visible = false;
            addChild(light);
        }

        public virtual void updateRotation()
        {
            float num = 140f;
            t1.x = x - num / 2f - 20f;
            t2.x = x + num / 2f - 20f;
            t1.y = t2.y = y;
            b1.x = t1.x;
            b2.x = t2.x;
            b1.y = b2.y = y + 15f;
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = vectRotateAround(t1, angle, x, y);
            t2 = vectRotateAround(t2, angle, x, y);
            b1 = vectRotateAround(b1, angle, x, y);
            b2 = vectRotateAround(b2, angle, x, y);
        }

        public override void draw()
        {
            Timeline timeline = light.getCurrentTimeline();
            if (timeline != null && timeline.state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                light.visible = false;
            }
            base.draw();
        }

        public override void drawBB()
        {
        }

        public override void update(float delta)
        {
            base.update(delta);
            if (mover != null)
            {
                updateRotation();
            }
        }

        public const float SOCK_IDLE_TIMOUT = 0.8f;

        public static int SOCK_RECEIVING = 0;

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
