using CutTheRope.iframework.core;
using CutTheRope.ios;
using CutTheRope.windows;
using System;

namespace CutTheRope.iframework.helpers
{
    internal class Camera2D : NSObject
    {
        public virtual Camera2D initWithSpeedandType(float s, CAMERA_TYPE t)
        {
            if (base.init() != null)
            {
                speed = s;
                type = t;
            }
            return this;
        }

        public virtual void moveToXYImmediate(float x, float y, bool immediate)
        {
            target.x = x;
            target.y = y;
            if (immediate)
            {
                pos = target;
                return;
            }
            if (type == CAMERA_TYPE.CAMERA_SPEED_DELAY)
            {
                offset = vectMult(vectSub(target, pos), speed);
                return;
            }
            if (type == CAMERA_TYPE.CAMERA_SPEED_PIXELS)
            {
                offset = vectMult(vectNormalize(vectSub(target, pos)), speed);
            }
        }

        public virtual void update(float delta)
        {
            if (!vectEqual(pos, target))
            {
                pos = vectAdd(pos, vectMult(offset, delta));
                pos = vect(round(pos.x), round(pos.y));
                if (!sameSign(offset.x, target.x - pos.x) || !sameSign(offset.y, target.y - pos.y))
                {
                    pos = target;
                }
            }
        }

        public virtual void applyCameraTransformation()
        {
            OpenGL.glTranslatef((double)(0f - pos.x), (double)(0f - pos.y), 0.0);
        }

        public virtual void cancelCameraTransformation()
        {
            OpenGL.glTranslatef(pos.x, pos.y, 0.0);
        }

        public CAMERA_TYPE type;

        public float speed;

        public Vector pos;

        public Vector target;

        public Vector offset;
    }
}
