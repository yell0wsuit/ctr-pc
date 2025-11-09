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
                this.speed = s;
                this.type = t;
            }
            return this;
        }

        public virtual void moveToXYImmediate(float x, float y, bool immediate)
        {
            this.target.x = x;
            this.target.y = y;
            if (immediate)
            {
                this.pos = this.target;
                return;
            }
            if (this.type == CAMERA_TYPE.CAMERA_SPEED_DELAY)
            {
                this.offset = CTRMathHelper.vectMult(CTRMathHelper.vectSub(this.target, this.pos), this.speed);
                return;
            }
            if (this.type == CAMERA_TYPE.CAMERA_SPEED_PIXELS)
            {
                this.offset = CTRMathHelper.vectMult(CTRMathHelper.vectNormalize(CTRMathHelper.vectSub(this.target, this.pos)), this.speed);
            }
        }

        public virtual void update(float delta)
        {
            if (!CTRMathHelper.vectEqual(this.pos, this.target))
            {
                this.pos = CTRMathHelper.vectAdd(this.pos, CTRMathHelper.vectMult(this.offset, delta));
                this.pos = CTRMathHelper.vect(CTRMathHelper.round((double)this.pos.x), CTRMathHelper.round((double)this.pos.y));
                if (!CTRMathHelper.sameSign(this.offset.x, this.target.x - this.pos.x) || !CTRMathHelper.sameSign(this.offset.y, this.target.y - this.pos.y))
                {
                    this.pos = this.target;
                }
            }
        }

        public virtual void applyCameraTransformation()
        {
            OpenGL.glTranslatef((double)(0f - this.pos.x), (double)(0f - this.pos.y), 0.0);
        }

        public virtual void cancelCameraTransformation()
        {
            OpenGL.glTranslatef((double)this.pos.x, (double)this.pos.y, 0.0);
        }

        public CAMERA_TYPE type;

        public float speed;

        public Vector pos;

        public Vector target;

        public Vector offset;
    }
}
