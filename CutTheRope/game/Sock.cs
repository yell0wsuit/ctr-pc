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
            return Sock.Sock_create(Application.getTexture(r));
        }

        public static Sock Sock_createWithResIDQuad(int r, int q)
        {
            Sock sock = Sock.Sock_create(Application.getTexture(r));
            sock.setDrawQuad(q);
            return sock;
        }

        public virtual void createAnimations()
        {
            this.light = Animation.Animation_createWithResID(85);
            this.light.anchor = 34;
            this.light.parentAnchor = 10;
            this.light.y = 270f;
            this.light.x = FrameworkTypes.RTD(0.0);
            this.light.addAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 2, new List<int> { 3, 4, 4 });
            this.light.doRestoreCutTransparency();
            this.light.visible = false;
            this.addChild(this.light);
        }

        public virtual void updateRotation()
        {
            float num = 140f;
            this.t1.x = this.x - num / 2f - 20f;
            this.t2.x = this.x + num / 2f - 20f;
            this.t1.y = this.t2.y = this.y;
            this.b1.x = this.t1.x;
            this.b2.x = this.t2.x;
            this.b1.y = this.b2.y = this.y + 15f;
            this.angle = (double)CTRMathHelper.DEGREES_TO_RADIANS(this.rotation);
            this.t1 = CTRMathHelper.vectRotateAround(this.t1, this.angle, this.x, this.y);
            this.t2 = CTRMathHelper.vectRotateAround(this.t2, this.angle, this.x, this.y);
            this.b1 = CTRMathHelper.vectRotateAround(this.b1, this.angle, this.x, this.y);
            this.b2 = CTRMathHelper.vectRotateAround(this.b2, this.angle, this.x, this.y);
        }

        public override void draw()
        {
            Timeline timeline = this.light.getCurrentTimeline();
            if (timeline != null && timeline.state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                this.light.visible = false;
            }
            base.draw();
        }

        public override void drawBB()
        {
        }

        public override void update(float delta)
        {
            base.update(delta);
            if (this.mover != null)
            {
                this.updateRotation();
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
