using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

namespace CutTheRope.Commons
{
    /// <summary>
    /// A horizontal scrollbar backed by a <see cref="Lift"/> that synchronises its position with a <see cref="ScrollableContainer"/>.
    /// </summary>
    internal sealed class HLiftScrollbar : Image
    {
        /// <summary>
        /// Initializes the scrollbar with a background quad, a lift (thumb) quad, and a pressed-state lift quad from a texture atlas.
        /// </summary>
        /// <param name="resourceName">Texture resource identifier.</param>
        /// <param name="bq">Background quad index.</param>
        /// <param name="lq">Lift (thumb) quad index for the normal state.</param>
        /// <param name="lqp">Lift (thumb) quad index for the pressed state.</param>
        /// <returns>The initialized scrollbar instance.</returns>
        public HLiftScrollbar InitWithResIDBackQuadLiftQuadLiftQuadPressed(string resourceName, int bq, int lq, int lqp)
        {
            if (InitWithTexture(Application.GetTexture(resourceName)) != null)
            {
                SetDrawQuad(bq);
                Image up = Image_createWithResIDQuad(resourceName, lq);
                Image image = Image_createWithResIDQuad(resourceName, lqp);
                Vector relativeQuadOffset = GetRelativeQuadOffset(resourceName, lq, lqp);
                image.x += relativeQuadOffset.X;
                image.y += relativeQuadOffset.Y;
                lift = (Lift)new Lift().InitWithUpElementDownElementandID(up, image, 0);
                lift.parentAnchor = 17;
                lift.anchor = 18;
                lift.minX = 1f;
                lift.maxX = width - lift.minX;
                lift.liftDelegate = new Lift.PercentXY(PercentXY);
                int touchExpandX = 45;
                lift.SetTouchIncreaseLeftRightTopBottom(touchExpandX, touchExpandX, -5f, 10f);
                _ = AddChild(lift);
                spointsNum = 0;
                spoints = null;
                activeSpoint = 0;
            }
            return this;
        }

        /// <summary>
        /// Returns the lift-space position of the scroll point at the given index.
        /// </summary>
        /// <param name="i">Scroll point index.</param>
        /// <returns>The scroll point position in lift-local coordinates.</returns>
        public Vector GetScrollPoint(int i)
        {
            return spoints[i];
        }

        /// <summary>
        /// Returns the total number of scroll points.
        /// </summary>
        /// <returns>The number of configured scroll points.</returns>
        public int GetTotalScrollPoints()
        {
            return spointsNum;
        }

        /// <summary>
        /// Recalculates the active scroll point based on the current lift position and notifies the delegate if it changed.
        /// </summary>
        public void UpdateActiveSpoint()
        {
            int i = 0;
            while (i < spointsNum)
            {
                if (lift.x <= spointsLimits[i].X)
                {
                    activeSpoint = limitPoints[i];
                    if (delegateLiftScrollbarDelegate != null)
                    {
                        delegateLiftScrollbarDelegate.ChangedActiveSpointFromTo(0, activeSpoint);
                        return;
                    }
                    break;
                }
                else
                {
                    i++;
                }
            }
        }

        /// <inheritdoc />
        public override void Update(float delta)
        {
            base.Update(delta);
            UpdateLift();
            for (int i = 0; i < spointsNum; i++)
            {
                if (lift.x <= spointsLimits[i].X)
                {
                    int candidateSpoint = limitPoints[i];
                    if (activeSpoint != candidateSpoint)
                    {
                        delegateLiftScrollbarDelegate?.ChangedActiveSpointFromTo(activeSpoint, candidateSpoint);
                        activeSpoint = candidateSpoint;
                    }
                    return;
                }
            }
            if (lift.x >= spointsLimits[spointsNum - 1].X && activeSpoint != limitPoints[spointsNum - 1])
            {
                delegateLiftScrollbarDelegate?.ChangedActiveSpointFromTo(activeSpoint, limitPoints[spointsNum - 1]);
                activeSpoint = limitPoints[spointsNum - 1];
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                spoints = null;
                spointsLimits = null;
                limitPoints = null;
                container = null;
                delegateLiftScrollbarDelegate = null;
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override bool OnTouchDownXY(float tx, float ty)
        {
            return base.OnTouchDownXY(tx, ty);
        }

        /// <inheritdoc />
        public override bool OnTouchUpXY(float tx, float ty)
        {
            bool flag = base.OnTouchUpXY(tx, ty);
            container.StartMovingToSpointInDirection(vectZero);
            return flag;
        }

        /// <summary>
        /// Callback invoked by the lift when its position changes; translates the percentage into a container scroll offset.
        /// </summary>
        /// <param name="px">Horizontal percentage (0..1).</param>
        /// <param name="py">Vertical percentage (unused).</param>
        public void PercentXY(float px, float py)
        {
            Vector maxScroll = container.GetMaxScroll();
            container.SetScroll(Vect(maxScroll.X * px, maxScroll.Y * py));
        }

        /// <summary>
        /// Synchronises the lift position to match the container's current scroll offset.
        /// </summary>
        public void UpdateLift()
        {
            Vector scroll = container.GetScroll();
            Vector maxScroll = container.GetMaxScroll();
            float scrollRatioX = 0f;
            if (maxScroll.X != 0f)
            {
                scrollRatioX = scroll.X / maxScroll.X;
            }
            if (maxScroll.Y != 0f)
            {
                _ = scroll.Y / maxScroll.Y;
            }
            lift.x = ((lift.maxX - lift.minX) * scrollRatioX) + lift.minX;
            lift.y = 0f;
        }

        /// <summary>
        /// Computes lift-space scroll point positions from the container and sorts them for boundary detection.
        /// </summary>
        public void CalcScrollPoints()
        {
            Vector maxScroll = container.GetMaxScroll();
            spointsNum = container.GetTotalScrollPoints();
            spoints = null;
            spointsLimits = null;
            limitPoints = null;
            spoints = new Vector[spointsNum];
            spointsLimits = new Vector[spointsNum];
            limitPoints = new int[spointsNum];
            for (int i = 0; i < spointsNum; i++)
            {
                Vector vector = VectNeg(container.GetScrollPoint(i));
                float scrollRatioX = 0f;
                if (maxScroll.X != 0f)
                {
                    scrollRatioX = vector.X / maxScroll.X;
                }
                if (maxScroll.Y != 0f)
                {
                    _ = vector.Y / maxScroll.Y;
                }
                float liftX = ((lift.maxX - lift.minX) * scrollRatioX) + lift.minX;
                spoints[i] = Vect(liftX, 0f);
            }
            for (int j = 0; j < spointsNum; j++)
            {
                spointsLimits[j] = spoints[j];
                limitPoints[j] = j;
            }
            bool flag = true;
            while (flag)
            {
                flag = false;
                for (int k = 0; k < spointsNum - 1; k++)
                {
                    if (spointsLimits[k].X > spointsLimits[k + 1].X)
                    {
                        flag = true;
                        (spointsLimits[k + 1], spointsLimits[k]) = (spointsLimits[k], spointsLimits[k + 1]);
                        (limitPoints[k + 1], limitPoints[k]) = (limitPoints[k], limitPoints[k + 1]);
                    }
                }
            }
            for (int l = 0; l < spointsNum - 1; l++)
            {
                Vector vector3 = spointsLimits[l];
                Vector vector4 = spointsLimits[l + 1];
                Vector[] array = spointsLimits;
                int limitIndex = l;
                array[limitIndex].X = array[limitIndex].X + ((vector4.X - vector3.X) / 2f);
            }
        }

        /// <summary>
        /// Assigns a scrollable container and recalculates scroll points and lift position.
        /// </summary>
        /// <param name="c">The scrollable container to bind to.</param>
        public void SetContainer(ScrollableContainer c)
        {
            container = c;
            if (container != null)
            {
                CalcScrollPoints();
                UpdateLift();
            }
        }

        /// <summary>
        /// Lift-space positions corresponding to each container scroll point.
        /// </summary>
        public Vector[] spoints;

        /// <summary>
        /// Sorted boundary positions used to determine which scroll point the lift is closest to.
        /// </summary>
        public Vector[] spointsLimits;

        /// <summary>
        /// Mapping from sorted boundary index back to the original scroll point index.
        /// </summary>
        public int[] limitPoints;

        /// <summary>
        /// Total number of scroll points.
        /// </summary>
        public int spointsNum;

        /// <summary>
        /// Index of the currently active scroll point.
        /// </summary>
        public int activeSpoint;

        /// <summary>
        /// The draggable lift (thumb) element.
        /// </summary>
        public Lift lift;

        /// <summary>
        /// The scrollable container this scrollbar is bound to.
        /// </summary>
        public ScrollableContainer container;

        /// <summary>
        /// Delegate notified when the active scroll point changes.
        /// </summary>
        public ILiftScrollbarDelegate delegateLiftScrollbarDelegate;
    }
}
