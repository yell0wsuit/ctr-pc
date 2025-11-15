using CutTheRope.iframework.core;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    internal sealed class Bouncer : CTRGameObject
    {
        public Bouncer InitWithPosXYWidthAndAngle(float px, float py, int w, double an)
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
            if (InitWithTexture(Application.GetTexture(textureResID)) == null)
            {
                return null;
            }
            rotation = (float)an;
            x = px;
            y = py;
            UpdateRotation();
            int i = AddAnimationDelayLoopFirstLast(0.04f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 4);
            GetTimeline(i).AddKeyFrame(KeyFrame.MakeSingleAction(this, "ACTION_SET_DRAWQUAD", 0, 0, 0.04f));
            return this;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (mover != null)
            {
                UpdateRotation();
            }
        }

        public void UpdateRotation()
        {
            t1.x = x - (width / 2);
            t2.x = x + (width / 2);
            t1.y = t2.y = (float)(y - 5.0);
            b1.x = t1.x;
            b2.x = t2.x;
            b1.y = b2.y = (float)(y + 5.0);
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = VectRotateAround(t1, angle, x, y);
            t2 = VectRotateAround(t2, angle, x, y);
            b1 = VectRotateAround(b1, angle, x, y);
            b2 = VectRotateAround(b2, angle, x, y);
        }

        public float angle;

        public Vector t1;

        public Vector t2;

        public Vector b1;

        public Vector b2;

        public bool skip;
    }
}
