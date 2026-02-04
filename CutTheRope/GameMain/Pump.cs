using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Represents a pump object that can apply a directional flow and be placed on conveyors.
    /// </summary>
    internal sealed class Pump : GameObject, IConveyorItem, IConveyorSizeProvider, IConveyorPaddingProvider, IConveyorPositionProvider, IConveyorPositionSetter
    {
        /// <summary>
        /// Length of the pump flow influence area in world units.
        /// </summary>
        public const float FlowLength = 624f;

        /// <summary>
        /// Offset from the pump center to the mouth in local X before rotation.
        /// </summary>
        public const float MouthOffset = 80f;

        private static readonly Vector ConveyorOffset = Vect(0.8f, -1.2f);

        /// <summary>
        /// Creates a pump from a texture.
        /// </summary>
        public static Pump Pump_create(CTRTexture2D t)
        {
            return (Pump)new Pump().InitWithTexture(t);
        }

        /// <summary>
        /// Creates a pump from a texture resource name.
        /// </summary>
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

        /// <summary>
        /// Updates the internal endpoints and angle based on the current rotation.
        /// </summary>
        public void UpdateRotation()
        {
            t1.X = x - (bb.w / 2f);
            t2.X = x + (bb.w / 2f);
            t1.Y = t2.Y = y;
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
            return (size.X + size.Y) / 4f;
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
            x = adjusted.X;
            y = adjusted.Y;
        }

        /// <summary>
        /// Current pump angle in radians.
        /// </summary>
        public double angle;

        public Vector t1;

        public Vector t2;

        /// <summary>
        /// Timer used for touch feedback.
        /// </summary>
        public float pumpTouchTimer;

        /// <summary>
        /// Touch state flag.
        /// </summary>
        public int pumpTouch;

        /// <summary>
        /// Initial rotation at placement time.
        /// </summary>
        public float initial_rotation;

        /// <summary>
        /// Initial X position at placement time.
        /// </summary>
        public float initial_x;

        /// <summary>
        /// Initial Y position at placement time.
        /// </summary>
        public float initial_y;

        public RotatedCircle initial_rotatedCircle;

        public int ConveyorId { get; set; } = -1;

        public float? ConveyorBaseScaleX { get; set; }

        public float? ConveyorBaseScaleY { get; set; }
    }
}
