using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal class Bouncer : CTRGameObject, IConveyorItem, IConveyorSizeProvider, IConveyorPaddingProvider, IConveyorPositionSetter
    {
        public virtual Bouncer InitWithPosXYWidthAndAngle(float px, float py, int w, double an)
        {
            string textureResourceName = w switch
            {
                SmallBouncerWidth => Resources.Img.ObjBouncer01,
                LargeBouncerWidth => Resources.Img.ObjBouncer02,
                _ => null
            };

            if (textureResourceName == null || InitWithTexture(Application.GetTexture(textureResourceName)) == null)
            {
                return null;
            }
            rotation = (float)an;
            x = px;
            y = py;
            UpdateRotation();
            int i = AddAnimationDelayLoopFirstLast(0.04f, Timeline.LoopType.TIMELINE_NO_LOOP, 0, 4);
            GetTimeline(i).AddKeyFrame(KeyFrame.MakeSingleAction(this, "ACTION_SET_DRAWQUAD", 0, 0, 0.04f));
            return this;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (mover != null)
            {
                UpdateRotation();
            }
        }

        public void UpdateRotation()
        {
            t1.x = x - (width / 2);
            t2.x = x + (width / 2);
            t1.y = t2.y = (float)(y - 5.0);
            b1.x = t1.x;
            b2.x = t2.x;
            b1.y = b2.y = (float)(y + 5.0);
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = VectRotateAround(t1, angle, x, y);
            t2 = VectRotateAround(t2, angle, x, y);
            b1 = VectRotateAround(b1, angle, x, y);
            b2 = VectRotateAround(b2, angle, x, y);
        }

        public Vector GetConveyorSize()
        {
            return Vect(width, height);
        }

        public float GetConveyorPadding()
        {
            Vector size = GetConveyorSize();
            return (size.x + size.y) / 4f;
        }

        public void SetConveyorPosition(Vector position)
        {
            x = position.x;
            y = position.y;
            UpdateRotation();
        }

        public float angle;

        public Vector t1;

        public Vector t2;

        public Vector b1;

        public Vector b2;

        public bool skip;

        public int ConveyorId { get; set; } = -1;

        public float? ConveyorBaseScaleX { get; set; }

        public float? ConveyorBaseScaleY { get; set; }

        private const int SmallBouncerWidth = 1;

        private const int LargeBouncerWidth = 2;
    }
}
