using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class Sock : CTRGameObject, ITransporterItem, ITransporterBindAware
    {
        private const float ScalingCompensation = 3f;
        private const float BindPointOffsetX = -3f * ScalingCompensation;
        private const float BindPointOffsetY = 25f * ScalingCompensation;

        public static Sock Sock_create(CTRTexture2D t)
        {
            return (Sock)new Sock().InitWithTexture(t);
        }

        public static Sock Sock_createWithResID(string resourceName)
        {
            return Sock_create(Application.GetTexture(resourceName));
        }

        /// <summary>
        /// Creates a sock using a texture resource name and quad index.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <param name="q">Quad index.</param>
        public static Sock Sock_createWithResIDQuad(string resourceName, int q)
        {
            Sock sock = Sock_create(Application.GetTexture(resourceName));
            sock.SetDrawQuad(q);
            return sock;
        }

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

        public override void Draw()
        {
            Timeline timeline = light.GetCurrentTimeline();
            if (timeline != null && timeline.state == Timeline.TimelineState.TIMELINE_STOPPED)
            {
                light.visible = false;
            }
            base.Draw();
        }

        public override void DrawBB()
        {
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (mover != null)
            {
                UpdateRotation();
            }
        }

        public const float SOCK_IDLE_TIMOUT = 0.8f;

        public const int SOCK_RECEIVING = 0;

        public const int SOCK_THROWING = 1;

        public const int SOCK_IDLE = 2;

        public int group;

        public float angle;

        public Vector t1;

        public Vector t2;

        public Vector b1;

        public Vector b2;

        public float idleTimeout;
        private string XmasSock;
        public Animation light;

        public float PositionOnTransporter { get; set; }

        /// <summary>
        /// Returns the effective position of the sock for transporter calculations,
        /// applying a scaled and rotated offset from the sock's origin to the mouth position.
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
        /// Sets the sock's position such that its effective transporter bind point
        /// matches the given position, accounting for the rotated offset.
        /// </summary>
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

        public float CollisionRadius => GetCollisionRadius();

        public float MinScale => 0.35f;

        public float MaxScale => 0.7f;

        public float TransporterScale { get; set; } = 1f;

        public bool IsDrawnByTransporter { get; set; }

        public void WillBind()
        {
            IsDrawnByTransporter = true;
        }

        private static float GetCollisionRadius()
        {
            return 30f * ScalingCompensation;
        }
    }
}
