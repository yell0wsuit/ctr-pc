using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using System;

namespace CutTheRope.game
{
    internal class Bouncer : CTRGameObject
    {
        private static Bouncer Bouncer_create(CTRTexture2D t)
        {
            return (Bouncer)new Bouncer().initWithTexture(t);
        }

        private static Bouncer Bouncer_createWithResID(int r)
        {
            return Bouncer.Bouncer_create(Application.getTexture(r));
        }

        private static Bouncer Bouncer_createWithResIDQuad(int r, int q)
        {
            Bouncer bouncer = Bouncer.Bouncer_create(Application.getTexture(r));
            bouncer.setDrawQuad(q);
            return bouncer;
        }

        public virtual NSObject initWithPosXYWidthAndAngle(float px, float py, int w, double an)
        {
            int textureResID = -1;
            if (w != 1)
            {
                if (w == 2)
                {
                    textureResID = 87;
                }
            }
            else
            {
                textureResID = 86;
            }
            if (this.initWithTexture(Application.getTexture(textureResID)) == null)
            {
                return null;
            }
            this.rotation = (float)an;
            this.x = px;
            this.y = py;
            this.updateRotation();
            int i = base.addAnimationDelayLoopFirstLast(0.04f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 4);
            this.getTimeline(i).addKeyFrame(KeyFrame.makeSingleAction(this, "ACTION_SET_DRAWQUAD", 0, 0, 0.04f));
            return this;
        }

        public override void update(float delta)
        {
            base.update(delta);
            if (this.mover != null)
            {
                this.updateRotation();
            }
        }

        public virtual void updateRotation()
        {
            this.t1.x = this.x - (float)(this.width / 2);
            this.t2.x = this.x + (float)(this.width / 2);
            this.t1.y = this.t2.y = (float)((double)this.y - 5.0);
            this.b1.x = this.t1.x;
            this.b2.x = this.t2.x;
            this.b1.y = this.b2.y = (float)((double)this.y + 5.0);
            this.angle = CTRMathHelper.DEGREES_TO_RADIANS(this.rotation);
            this.t1 = CTRMathHelper.vectRotateAround(this.t1, (double)this.angle, this.x, this.y);
            this.t2 = CTRMathHelper.vectRotateAround(this.t2, (double)this.angle, this.x, this.y);
            this.b1 = CTRMathHelper.vectRotateAround(this.b1, (double)this.angle, this.x, this.y);
            this.b2 = CTRMathHelper.vectRotateAround(this.b2, (double)this.angle, this.x, this.y);
        }

        private const float BOUNCER_HEIGHT = 10f;

        public float angle;

        public Vector t1;

        public Vector t2;

        public Vector b1;

        public Vector b2;

        public bool skip;
    }
}
