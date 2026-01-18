using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class Pump : GameObject, IConveyorItem, IConveyorSizeProvider, IConveyorPaddingProvider, IConveyorPositionProvider, IConveyorPositionSetter
    {
        private static readonly Vector ConveyorOffset = Vect(0.8f, -1.2f);

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

        public Vector GetConveyorSize()
        {
            const float scale = 0.48f;
            return Vect(bb.w * scale, bb.h * scale);
        }

        public float GetConveyorPadding()
        {
            Vector size = GetConveyorSize();
            return (size.x + size.y) / 4f;
        }

        public Vector GetConveyorPosition()
        {
            Vector offset = VectRotate(ConveyorOffset, angle);
            return VectAdd(Vect(x, y), offset);
        }

        public void SetConveyorPosition(Vector position)
        {
            Vector offset = VectRotate(ConveyorOffset, angle);
            Vector adjusted = VectSub(position, offset);
            x = adjusted.x;
            y = adjusted.y;
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

        public int ConveyorId { get; set; } = -1;

        public float? ConveyorBaseScaleX { get; set; }

        public float? ConveyorBaseScaleY { get; set; }
    }
}
