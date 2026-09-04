using CutTheRopeDX.Framework;
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
        /// Creates the teleport flash animation from the same art the hat itself draws.
        /// </summary>
        /// <remarks>
        /// The resource is passed in rather than read from the season, because a group past the
        /// Christmas socks falls back to the magic hat and its flash has to come from there too.
        /// </remarks>
        /// <param name="resourceName">Texture resource this hat draws from.</param>
        public void CreateAnimations(string resourceName)
        {
            XmasSock = resourceName;
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
        /// Gives the hat a colored band, for groups the shipped art bakes no color for.
        /// </summary>
        /// <remarks>
        /// Two layers over the base frame: an opaque backdrop that paints out the band the frame
        /// already carries, then the grayscale mask over it. The mask keeps its shading and takes
        /// the group's color from the renderer's own tint, which is why nothing here has to build a
        /// recolored texture. Both are children, so they follow the hat as it turns, shrinks onto a
        /// transporter, or travels its path.
        /// </remarks>
        /// <param name="pattern">Band pattern authored for the base frame this hat draws.</param>
        /// <param name="color">Color the band wears.</param>
        public void CreateBand(int pattern, RGBAColor color)
        {
            BandBackdrop = AddBandLayer(pattern * 2);
            Band = AddBandLayer((pattern * 2) + 1);
            Band.color = color;
            Band.useFullColorTint = true;
        }

        /// <summary>Adds one band layer, aligned to the base frame by its own atlas offset.</summary>
        /// <param name="quad">Quad to draw from the maskable band atlas.</param>
        /// <returns>The added layer.</returns>
        private Image AddBandLayer(int quad)
        {
            Image layer = Image_createWithResIDQuad(Resources.Img.ObjHatMaskable, quad);

            // Anchored to the hat's own top-left corner: both atlases place their frames within the
            // same source drawing, so each layer landing on its own offset lands where it was drawn.
            layer.anchor = 9;
            layer.parentAnchor = 9;
            layer.DoRestoreCutTransparency();
            _ = AddChild(layer);
            return layer;
        }

        /// <summary>
        /// Recomputes the magic hat rotated mouth bounds from the current position and rotation.
        /// </summary>
        public void UpdateRotation()
        {
            float mouthHalfWidth;
            float mouthOffsetX;
            float mouthDepth;
            if (ActivePhysicsConstants.UseMobilePhysicsModel)
            {
                // WP7 mouth: x +/- 15, y .. y + 1, scaled x3 into world units.
                mouthHalfWidth = 45f;
                mouthOffsetX = 0f;
                mouthDepth = 3f;
            }
            else
            {
                mouthHalfWidth = 70f;
                mouthOffsetX = -20f;
                mouthDepth = 15f;
            }
            t1.X = x - mouthHalfWidth + mouthOffsetX;
            t2.X = x + mouthHalfWidth + mouthOffsetX;
            t1.Y = t2.Y = y;
            b1.X = t1.X;
            b2.X = t2.X;
            b1.Y = b2.Y = y + mouthDepth;
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

        /// <summary>Layer that paints out the band baked into the base frame, or <see langword="null"/> for an authored group.</summary>
        public Image BandBackdrop { get; private set; }

        /// <summary>Tinted band layer, or <see langword="null"/> for a group whose color is baked in.</summary>
        public Image Band { get; private set; }

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
