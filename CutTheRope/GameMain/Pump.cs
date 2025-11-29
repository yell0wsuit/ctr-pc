using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class Pump : GameObject
    {
        public static Pump Pump_create(CTRTexture2D t)
        {
            return (Pump)new Pump().InitWithTexture(t);
        }

        public static Pump Pump_createWithResID(int r)
        {
            return Pump_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
        }

        public static Pump Pump_createWithResID(string resourceName)
        {
            return Pump_create(Application.GetTexture(resourceName));
        }

        /// <summary>
        /// Creates a pump using a texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <param name="q">Quad index to draw.</param>
        public static Pump Pump_createWithResID(string resourceName, int q)
        {
            Pump pump = Pump_create(Application.GetTexture(resourceName));
            pump.SetDrawQuad(q);
            return pump;
        }

        public void UpdateRotation()
        {
            t1.x = x - (bb.w / 2f);
            t2.x = x + (bb.w / 2f);
            t1.y = t2.y = y;
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = VectRotateAround(t1, angle, x, y);
            t2 = VectRotateAround(t2, angle, x, y);
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
