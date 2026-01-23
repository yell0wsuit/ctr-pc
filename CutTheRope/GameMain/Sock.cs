using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class Sock : CTRGameObject, IConveyorItem, IConveyorSizeProvider, IConveyorPaddingProvider, IConveyorPositionProvider, IConveyorPositionSetter
    {
        private const float SockConveyorSizeScale = 0.28f;
        private static readonly Vector SockConveyorOffset = Vect(-2.1f, 15f);
        private const float ConveyorPm = 1.2f;

        public static Sock Sock_create(CTRTexture2D t)
        {
            return (Sock)new Sock().InitWithTexture(t);
        }

        public static Sock Sock_createWithResID(int r)
        {
            return Sock_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
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
            light.x = RTD(0.0);
            light.AddAnimationWithIDDelayLoopCountSequence(0, 0.05f, Timeline.LoopType.TIMELINE_NO_LOOP, 4, 2, [3, 4, 4]);
            light.DoRestoreCutTransparency();
            light.visible = false;
            _ = AddChild(light);
        }

        public void UpdateRotation()
        {
            float num = 140f;
            t1.x = x - (num / 2f) - 20f;
            t2.x = x + (num / 2f) - 20f;
            t1.y = t2.y = y;
            b1.x = t1.x;
            b2.x = t2.x;
            b1.y = b2.y = y + 15f;
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

        public Vector GetConveyorSize()
        {
            return Vect(width * SockConveyorSizeScale, height * SockConveyorSizeScale);
        }

        public float GetConveyorPadding()
        {
            Vector size = GetConveyorSize();
            return (size.x + size.y) / 4f;
        }

        public Vector GetConveyorPosition()
        {
            float pmScale = RotatedCircle.PM / ConveyorPm;
            Vector offset = Vect(SockConveyorOffset.x * pmScale, SockConveyorOffset.y * pmScale);
            // offset = VectRotate(offset, DEGREES_TO_RADIANS(rotation));
            offset = VectRotate(offset, angle);
            return VectAdd(Vect(x, y), offset);
        }

        public void SetConveyorPosition(Vector position)
        {
            float pmScale = RotatedCircle.PM / ConveyorPm;
            Vector offset = Vect(SockConveyorOffset.x * pmScale, SockConveyorOffset.y * pmScale);
            // offset = VectRotate(offset, DEGREES_TO_RADIANS(rotation));
            offset = VectRotate(offset, angle);
            Vector adjusted = VectSub(position, offset);
            x = adjusted.x;
            y = adjusted.y;
        }

        public const float SOCK_IDLE_TIMOUT = 0.8f;

        public const int SOCK_RECEIVING = 0;

        public const int SOCK_THROWING = 1;

        public const int SOCK_IDLE = 2;

        public int group;

        public double angle;

        public Vector t1;

        public Vector t2;

        public Vector b1;

        public Vector b2;

        public float idleTimeout;
        private string XmasSock;
        public Animation light;

        public int ConveyorId { get; set; } = -1;

        public float? ConveyorBaseScaleX { get; set; }

        public float? ConveyorBaseScaleY { get; set; }
    }
}
