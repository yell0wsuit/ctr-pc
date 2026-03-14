using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal class Bouncer : CTRGameObject, ITransporterItem, ITransporterBindAware, ITransporterSideSwitchAware
    {
        public virtual Bouncer InitWithPosXYWidthAndAngle(float px, float py, int w, float an)
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
            rotation = an;
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
            t1.X = x - (width / 2);
            t2.X = x + (width / 2);
            t1.Y = t2.Y = y - ActivePhysicsConstants.BouncerHeight;
            b1.X = t1.X;
            b2.X = t2.X;
            b1.Y = b2.Y = y + ActivePhysicsConstants.BouncerHeight;
            angle = DEGREES_TO_RADIANS(rotation);
            t1 = VectRotateAround(t1, angle, x, y);
            t2 = VectRotateAround(t2, angle, x, y);
            b1 = VectRotateAround(b1, angle, x, y);
            b2 = VectRotateAround(b2, angle, x, y);
        }

        public float angle;

        public Vector t1;

        public Vector t2;

        public Vector b1;

        public Vector b2;

        public bool skip;

        public Vector prevPosition2;

        public void DidMoveToOtherSide()
        {
            prevPosition2 = Vect(x, y);
        }

        public void WillBind()
        {
            IsDrawnByTransporter = true;
        }

        public float PositionOnTransporter { get; set; }

        public Vector BindPoint => Vect(x, y);

        public void SetBindPoint(Vector point)
        {
            x = point.X;
            y = point.Y;
            UpdateRotation();
        }

        public float CollisionRadius => width * 0.5f;

        public float MinScale => 0.5f;

        public float MaxScale => 1.0f;

        public float TransporterScale { get; set; } = 1.0f;

        public bool IsDrawnByTransporter { get; set; }

        private const int SmallBouncerWidth = 1;

        private const int LargeBouncerWidth = 2;
    }
}
