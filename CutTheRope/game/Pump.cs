using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using System;

namespace CutTheRope.game
{
    internal class Pump : GameObject
    {
        public static Pump Pump_create(CTRTexture2D t)
        {
            return (Pump)new Pump().initWithTexture(t);
        }

        public static Pump Pump_createWithResID(int r)
        {
            return Pump.Pump_create(Application.getTexture(r));
        }

        public static Pump Pump_createWithResIDQuad(int r, int q)
        {
            Pump pump = Pump.Pump_create(Application.getTexture(r));
            pump.setDrawQuad(q);
            return pump;
        }

        public virtual void updateRotation()
        {
            this.t1.x = this.x - this.bb.w / 2f;
            this.t2.x = this.x + this.bb.w / 2f;
            this.t1.y = this.t2.y = this.y;
            this.angle = (double)CTRMathHelper.DEGREES_TO_RADIANS(this.rotation);
            this.t1 = CTRMathHelper.vectRotateAround(this.t1, this.angle, this.x, this.y);
            this.t2 = CTRMathHelper.vectRotateAround(this.t2, this.angle, this.x, this.y);
        }

        public double angle;

        public Vector t1;

        public Vector t2;

        public float pumpTouchTimer;

        public int pumpTouch;

        public float initial_rotation;

        public float initial_x;

        public float initial_y;

        public RotatedCircle initial_rotatedCircle;
    }
}
