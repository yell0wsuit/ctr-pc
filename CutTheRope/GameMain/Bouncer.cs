using System;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal class Bouncer : CTRGameObject, ITransporterItem, ITransporterBindAware, ITransporterSideSwitchAware
    {
        public virtual Bouncer InitWithPosXYWidthAndAngle(float px, float py, int w, float an)
        {
            int firstQuad = w switch
            {
                SmallBouncerWidth => SmallBouncerFirstQuad,
                LargeBouncerWidth => LargeBouncerFirstQuad,
                _ => -1
            };

            if (firstQuad == -1 || InitWithTexture(Application.GetTexture(Resources.Img.ObjBouncer)) == null)
            {
                return null;
            }
            SetDrawQuad(firstQuad);
            rotation = an;
            x = px;
            y = py;
            UpdateRotation();
            int lastQuad = firstQuad + 4;
            int i = AddAnimationDelayLoopFirstLast(0.04f, Timeline.LoopType.TIMELINE_NO_LOOP, firstQuad, lastQuad);
            GetTimeline(i).AddKeyFrame(KeyFrame.MakeSingleAction(this, "ACTION_SET_DRAWQUAD", firstQuad, 0, 0.04f));
            return this;
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (mover != null)
            {
                UpdateRotation();
            }

            if (timeElapsedFromLastMove >= 0f)
            {
                timeElapsedFromLastMove += delta;
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
            float dx = point.X - x;
            float dy = point.Y - y;
            float distSq = (dx * dx) + (dy * dy);

            if (distSq < 0.000001f)
            {
                return;
            }

            if (timeElapsedFromLastMove is < 0.001f or > 0.1f)
            {
                instantVelocity = Vect(0f, 0f);
            }
            else
            {
                float vx = dx / timeElapsedFromLastMove;
                float vy = dy / timeElapsedFromLastMove;
                float magSq = (vx * vx) + (vy * vy);

                if (magSq > MaxVelocityMagnitude * MaxVelocityMagnitude)
                {
                    float invMag = 1f / MathF.Sqrt(magSq);
                    vx *= invMag * MaxVelocityMagnitude;
                    vy *= invMag * MaxVelocityMagnitude;
                }

                instantVelocity = Vect(vx, vy);
            }

            timeElapsedFromLastMove = 0f;
            prevPosition2 = Vect(x, y);
            x = point.X;
            y = point.Y;
            UpdateRotation();
        }

        public float CollisionRadius => 40f;

        public float MinScale => 0.5f;

        public float MaxScale => 1.0f;

        public float TransporterScale { get; set; } = 1.0f;

        public bool IsDrawnByTransporter { get; set; }

        private const int SmallBouncerWidth = 1;

        private const int LargeBouncerWidth = 2;

        private const int SmallBouncerFirstQuad = 0;

        private const int LargeBouncerFirstQuad = 5;

        private const float MaxVelocityMagnitude = 200f;

        private float timeElapsedFromLastMove = -1f;
        public Vector instantVelocity;
    }
}
