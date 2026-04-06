using System;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// A bouncy surface that makes the candy bounce on contact.
    /// Can be small (1-unit) or large (2-unit) and optionally ride a transporter belt.
    /// </summary>
    internal class Bouncer : CTRGameObject, ITransporterItem, ITransporterBindAware, ITransporterSideSwitchAware
    {
        /// <summary>
        /// Initialises the bouncer at the given position with a width class and rotation angle.
        /// </summary>
        /// <param name="px">World-space X position.</param>
        /// <param name="py">World-space Y position.</param>
        /// <param name="w">Width class: <see cref="SmallBouncerWidth"/> (1) or <see cref="LargeBouncerWidth"/> (2).</param>
        /// <param name="an">Initial rotation angle in degrees.</param>
        /// <returns>This instance on success, or <see langword="null"/> if the width is invalid or the texture failed to load.</returns>
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

        /// <inheritdoc />
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

        /// <summary>
        /// Recomputes the four rotated corner points (<see cref="t1"/>, <see cref="t2"/>,
        /// <see cref="b1"/>, <see cref="b2"/>) from the current position, width and rotation.
        /// </summary>
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

        /// <summary>Current rotation in radians, derived from <see cref="BaseElement.rotation"/>.</summary>
        public float angle;

        /// <summary>Top-left corner of the bouncer rectangle in world space (after rotation).</summary>
        public Vector t1;

        /// <summary>Top-right corner of the bouncer rectangle in world space (after rotation).</summary>
        public Vector t2;

        /// <summary>Bottom-left corner of the bouncer rectangle in world space (after rotation).</summary>
        public Vector b1;

        /// <summary>Bottom-right corner of the bouncer rectangle in world space (after rotation).</summary>
        public Vector b2;

        /// <summary>When <see langword="true"/>, collision checks skip this bouncer for the current frame.</summary>
        public bool skip;

        /// <summary>Position before the most recent <see cref="SetBindPoint"/> or <see cref="DidMoveToOtherSide"/> call.</summary>
        public Vector prevPosition2;

        /// <inheritdoc />
        public void DidMoveToOtherSide()
        {
            prevPosition2 = Vect(x, y);
        }

        /// <inheritdoc />
        public void WillBind()
        {
            IsDrawnByTransporter = true;
        }

        /// <inheritdoc />
        public float PositionOnTransporter { get; set; }

        /// <inheritdoc />
        public Vector BindPoint => Vect(x, y);

        /// <inheritdoc />
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

        /// <inheritdoc />
        public float CollisionRadius => 40f;

        /// <inheritdoc />
        public float MinScale => 0.5f;

        /// <inheritdoc />
        public float MaxScale => 1.0f;

        /// <inheritdoc />
        public float TransporterScale { get; set; } = 1.0f;

        /// <inheritdoc />
        public bool IsDrawnByTransporter { get; set; }

        /// <summary>Width class value for the small bouncer variant.</summary>
        private const int SmallBouncerWidth = 1;

        /// <summary>Width class value for the large bouncer variant.</summary>
        private const int LargeBouncerWidth = 2;

        /// <summary>First texture quad index for the small bouncer animation.</summary>
        private const int SmallBouncerFirstQuad = 0;

        /// <summary>First texture quad index for the large bouncer animation.</summary>
        private const int LargeBouncerFirstQuad = 5;

        /// <summary>Maximum allowed instantaneous velocity magnitude (world units per second).</summary>
        private const float MaxVelocityMagnitude = 200f;

        /// <summary>Seconds since the last <see cref="SetBindPoint"/> move, or −1 if never moved.</summary>
        private float timeElapsedFromLastMove = -1f;

        /// <summary>Instantaneous velocity computed from the most recent <see cref="SetBindPoint"/> displacement.</summary>
        public Vector instantVelocity;
    }
}
