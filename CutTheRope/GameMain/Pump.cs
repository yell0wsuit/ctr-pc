using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Represents a pump object that can apply a directional flow and be placed on conveyors.
    /// </summary>
    internal sealed class Pump : GameObject, ITransporterItem, ITransporterBindAware
    {
        /// <summary>
        /// Length of the pump flow influence area in world units.
        /// </summary>
        public const float FlowLength = 624f;

        /// <summary>
        /// Offset from the pump center to the mouth in local X before rotation.
        /// </summary>
        public const float MouthOffset = 80f;

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

        /// <summary>
        /// Current pump angle in radians.
        /// </summary>
        public float angle;

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

        public float PositionOnTransporter { get; set; }

        /// <summary>
        /// Returns the effective position of the pump for transporter calculations,
        /// applying a rotated ConveyorOffset from the pump's origin.
        /// </summary>
        public Vector BindPoint
        {
            get
            {
                float angleRad = DEGREES_TO_RADIANS(rotation);
                Vector offset = VectRotate(Vect(width * 0.01f * scaleX, 0f), angleRad);
                return VectAdd(Vect(x, y), offset);
            }
        }

        /// <summary>
        /// Sets the pump's position such that its effective transporter bind point
        /// matches the given position, accounting for the rotated offset.
        /// </summary>
        public void SetBindPoint(Vector point)
        {
            float angleRad = DEGREES_TO_RADIANS(rotation);
            Vector offset = VectRotate(Vect(width * 0.01f * scaleX, 0f), angleRad);
            Vector adjusted = VectSub(point, offset);
            x = adjusted.X;
            y = adjusted.Y;
        }

        public float CollisionRadius => width * 0.13f;

        public float MinScale => 0.5f;

        public float MaxScale => 1.0f;

        public float TransporterScale { get; set; } = 1.0f;

        public bool IsDrawnByTransporter { get; set; }

        public void WillBind()
        {
            IsDrawnByTransporter = true;
        }
    }
}
