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
            return Pump_create(Application.getTexture(r));
        }

        public static Pump Pump_createWithResIDQuad(int r, int q)
        {
            Pump pump = Pump_create(Application.getTexture(r));
            pump.setDrawQuad(q);
            return pump;
        }

        public virtual void updateRotation()
        {
            t1.x = x - bb.w / 2f;
            t2.x = x + bb.w / 2f;
            t1.y = t2.y = y;
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = vectRotateAround(t1, angle, x, y);
            t2 = vectRotateAround(t2, angle, x, y);
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
