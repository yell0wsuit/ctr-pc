using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Magic hat teleporter object, rendered as a Christmas sock during the seasonal theme.
    /// </summary>
    internal sealed class Sock : CTRGameObject, ITransporterItem, ITransporterBindAware
    {
        /// <summary>Scale factor used to convert magic hat offsets into world units.</summary>
        private const float ScalingCompensation = 3f;

        /// <summary>Local X offset from object origin to transporter bind point.</summary>
        private const float BindPointOffsetX = -3f * ScalingCompensation;

        /// <summary>Local Y offset from object origin to transporter bind point.</summary>
        private const float BindPointOffsetY = 25f * ScalingCompensation;

        /// <summary>
        /// Creates a magic hat from a texture.
        /// </summary>
        /// <param name="t">Texture used by the magic hat.</param>
        /// <returns>The initialized magic hat.</returns>
        public static Sock Sock_create(CTRTexture2D t)
        {
            return (Sock)new Sock().InitWithTexture(t);
        }

        /// <summary>
        /// Creates a magic hat from a texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <returns>The initialized magic hat.</returns>
        public static Sock Sock_createWithResID(string resourceName)
        {
            return Sock_create(Application.GetTexture(resourceName));
        }

        /// <summary>
        /// Creates a magic hat using a texture resource name and quad index.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <param name="q">Quad index.</param>
        /// <returns>The initialized magic hat.</returns>
        public static Sock Sock_createWithResIDQuad(string resourceName, int q)
        {
            Sock sock = Sock_create(Application.GetTexture(resourceName));
            sock.SetDrawQuad(q);
            return sock;
        }

        /// <summary>
        /// Creates the magic hat teleport flash animation, using the Christmas sock resource during the seasonal theme.
        /// </summary>
        public void CreateAnimations()
        {
            XmasSock = SpecialEvents.IsXmas ? Resources.Img.ObjSock : Resources.Img.ObjHat;
            light = Animation_createWithResID(XmasSock);
            light.anchor = 34;
            light.parentAnchor = 10;
            light.y = 270f;
            light.x = RTD(0);
            light.AddAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 2, [3, 4, 4]);
            light.DoRestoreCutTransparency();
            light.visible = false;
            _ = AddChild(light);
        }

        /// <summary>
        /// Recomputes the magic hat rotated mouth bounds from the current position and rotation.
        /// </summary>
        public void UpdateRotation()
        {
            float sockWidth = 140f;
            t1.X = x - (sockWidth / 2f) - 20f;
            t2.X = x + (sockWidth / 2f) - 20f;
            t1.Y = t2.Y = y;
            b1.X = t1.X;
            b2.X = t2.X;
            b1.Y = b2.Y = y + 15f;
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = VectRotateAround(t1, angle, x, y);
            t2 = VectRotateAround(t2, angle, x, y);
            b1 = VectRotateAround(b1, angle, x, y);
            b2 = VectRotateAround(b2, angle, x, y);
        }

        /// <inheritdoc />
        public override void Draw()
        {
            Timeline timeline = light.GetCurrentTimeline();
            if (timeline != null && timeline.state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                light.visible = false;
            }
            base.Draw();
        }

        /// <inheritdoc />
        public override void DrawBB()
        {
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            if (mover != null)
            {
                UpdateRotation();
            }
        }

        /// <summary>Time in seconds before the magic hat returns to idle.</summary>
        public const float SOCK_IDLE_TIMOUT = 0.8f;

        /// <summary>State value used while the magic hat is receiving an object.</summary>
        public const int SOCK_RECEIVING = 0;

        /// <summary>State value used while the magic hat is throwing an object out.</summary>
        public const int SOCK_THROWING = 1;

        /// <summary>Idle magic hat state value.</summary>
        public const int SOCK_IDLE = 2;

        /// <summary>Teleport group identifier used to pair magic hats.</summary>
        public int group;

        /// <summary>Current magic hat angle in radians.</summary>
        public float angle;

        /// <summary>Top-left rotated mouth bound point.</summary>
        public Vector t1;

        /// <summary>Top-right rotated mouth bound point.</summary>
        public Vector t2;

        /// <summary>Bottom-left rotated mouth bound point.</summary>
        public Vector b1;

        /// <summary>Bottom-right rotated mouth bound point.</summary>
        public Vector b2;

        /// <summary>Remaining idle timeout in seconds.</summary>
        public float idleTimeout;

        /// <summary>Current visual resource used by the magic hat or Christmas sock theme.</summary>
        private string XmasSock;

        /// <summary>Teleport flash animation shown when an object exits the magic hat.</summary>
        public Animation light;

        /// <inheritdoc />
        public float PositionOnTransporter { get; set; }

        /// <summary>
        /// Returns the effective position of the magic hat for transporter calculations,
        /// applying a scaled and rotated offset from the origin to the mouth position.
        /// </summary>
        public Vector BindPoint
        {
            get
            {
                float bindPointOffsetX = BindPointOffsetX;
                float bindPointOffsetY = BindPointOffsetY;
                Vector offset = Vect(bindPointOffsetX * scaleX, bindPointOffsetY * scaleY);
                offset = VectRotate(offset, angle);
                return VectAdd(Vect(x, y), offset);
            }
        }

        /// <summary>
        /// Sets the magic hat position such that its effective transporter bind point
        /// matches the given position, accounting for the rotated offset.
        /// </summary>
        /// <param name="point">Target world-space bind point.</param>
        public void SetBindPoint(Vector point)
        {
            float bindPointOffsetX = BindPointOffsetX;
            float bindPointOffsetY = BindPointOffsetY;
            Vector offset = Vect(bindPointOffsetX * scaleX, bindPointOffsetY * scaleY);
            offset = VectRotate(offset, angle);
            Vector adjusted = VectSub(point, offset);
            x = adjusted.X;
            y = adjusted.Y;
        }

        /// <inheritdoc />
        public float CollisionRadius => GetCollisionRadius();

        /// <inheritdoc />
        public float MinScale => 0.35f;

        /// <inheritdoc />
        public float MaxScale => 0.7f;

        /// <inheritdoc />
        public float TransporterScale { get; set; } = 1f;

        /// <inheritdoc />
        public bool IsDrawnByTransporter { get; set; }

        /// <inheritdoc />
        public void WillBind()
        {
            IsDrawnByTransporter = true;
        }

        /// <summary>
        /// Gets the transporter collision radius for magic hat instances.
        /// </summary>
        /// <returns>The collision radius in world units.</returns>
        private static float GetCollisionRadius()
        {
            return 30f * ScalingCompensation;
        }
    }
}
