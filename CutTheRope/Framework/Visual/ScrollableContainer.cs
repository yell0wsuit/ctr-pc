using System;
using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;

namespace CutTheRope.Framework.Visual
{
    internal sealed class ScrollableContainer : BaseElement
    {
        public void ProvideScrollPosMaxScrollPosScrollCoeff(ref Vector sp, ref Vector mp, ref Vector sc)
        {
            sp = GetScroll();
            mp = GetMaxScroll();
            float scrollCoeffX = container.width / (float)width;
            float scrollCoeffY = container.height / (float)height;
            sc = Vect(scrollCoeffX, scrollCoeffY);
        }

        public override int AddChildwithID(BaseElement c, int i)
        {
            int childId = container.AddChildwithID(c, i);
            c.parentAnchor = 9;
            return childId;
        }

        public override int AddChild(BaseElement c)
        {
            c.parentAnchor = 9;
            return container.AddChild(c);
        }

        public override void RemoveChildWithID(int i)
        {
            container.RemoveChildWithID(i);
        }

        public override void RemoveChild(BaseElement c)
        {
            container.RemoveChild(c);
        }

        public override BaseElement GetChild(int i)
        {
            return container.GetChild(i);
        }

        public override int ChildsCount()
        {
            return container.ChildsCount();
        }

        public override void Draw()
        {
            PreDraw();
            Renderer.Enable(Renderer.GL_SCISSOR_TEST);
            Renderer.SetScissor(drawX, drawY, width, height);
            PostDraw();
            Renderer.Disable(Renderer.GL_SCISSOR_TEST);
        }

        public override void PostDraw()
        {
            if (!passTransformationsToChilds)
            {
                RestoreTransformations(this);
            }
            container.PreDraw();
            if (!container.passTransformationsToChilds)
            {
                RestoreTransformations(container);
            }
            Dictionary<int, BaseElement> dictionary = container.GetChilds();
            int i = 0;
            int count = dictionary.Count;
            while (i < count)
            {
                BaseElement baseElement = dictionary[i];
                float childDrawX = baseElement.drawX;
                float childDrawY = baseElement.drawY;
                if (baseElement != null && baseElement.visible && RectInRect(childDrawX, childDrawY, childDrawX + baseElement.width, childDrawY + baseElement.height, drawX, drawY, drawX + width, drawY + height))
                {
                    baseElement.Draw();
                }
                else
                {
                    CalculateTopLeft(baseElement);
                }
                i++;
            }
            if (container.passTransformationsToChilds)
            {
                RestoreTransformations(container);
            }
            if (passTransformationsToChilds)
            {
                RestoreTransformations(this);
            }
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            targetPoint = vectZero;
            if (touchTimer > 0f)
            {
                touchTimer -= delta;
                if (touchTimer <= 0f)
                {
                    touchTimer = 0f;
                    passTouches = true;
                    if (base.OnTouchDownXY(savedTouch.X, savedTouch.Y))
                    {
                        return;
                    }
                }
            }
            if (touchReleaseTimer > 0f)
            {
                touchReleaseTimer -= delta;
                if (touchReleaseTimer <= 0f)
                {
                    touchReleaseTimer = 0f;
                    if (base.OnTouchUpXY(savedTouch.X, savedTouch.Y))
                    {
                        return;
                    }
                }
            }
            if (touchState == TOUCH_STATE.UP)
            {
                if (shouldBounceHorizontally)
                {
                    if (container.x > 0)
                    {
                        float speed = 50 + (Math.Abs(container.x) * 5);
                        MoveToPointDeltaSpeed(Vect(0f, container.y), delta, speed);
                    }
                    else if (container.x < (-container.width + width) && container.x < 0)
                    {
                        float speed2 = 50 + (Math.Abs(-container.width + width - container.x) * 5);
                        MoveToPointDeltaSpeed(Vect(-container.width + width, container.y), delta, speed2);
                    }
                }
                if (shouldBounceVertically)
                {
                    if (container.y > 0)
                    {
                        MoveToPointDeltaSpeed(Vect(container.x, 0f), delta, 50f + (Math.Abs(container.y) * 5f));
                    }
                    else if (container.y < (-container.height + height) && container.y < 0f)
                    {
                        MoveToPointDeltaSpeed(Vect(container.x, -container.height + height), delta, 50f + (Math.Abs(-container.height + height - container.y) * 5f));
                    }
                }
            }
            if (movingToSpoint)
            {
                Vector vector = spoints[targetSpoint];
                MoveToPointDeltaSpeed(vector, delta, Math.Max(100f, VectDistance(vector, Vect(container.x, container.y)) * 4f * spointMoveMultiplier));
                if (container.x == vector.X && container.y == vector.Y)
                {
                    delegateScrollableContainerProtocol?.ScrollableContainerreachedScrollPoint(this, targetSpoint);
                    movingToSpoint = false;
                    targetSpoint = -1;
                    lastTargetSpoint = -1;
                    move = vectZero;
                }
            }
            else if (canSkipScrollPoints && spointsNum > 0 && !VectEqual(move, vectZero) && VectLength(move) < 150f && targetSpoint == -1)
            {
                StartMovingToSpointInDirection(move);
            }
            if (!VectEqual(move, vectZero))
            {
                _ = VectEqual(targetPoint, vectZero);
                _ = Vect(container.x, container.y);
                Vector v = VectMult(VectNeg(move), 7f); // Decelerate faster after scrolling
                move = VectAdd(move, VectMult(v, delta));
                Vector off = VectMult(move, delta);
                if (Math.Abs(off.X) < 0.2f)
                {
                    off.X = 0f;
                    move.X = 0f;
                }
                if (Math.Abs(off.Y) < 0.2f)
                {
                    off.Y = 0f;
                    move.Y = 0f;
                }
                _ = MoveContainerBy(off);
            }
            if (inertiaTimeoutLeft > 0f)
            {
                inertiaTimeoutLeft -= delta;
            }
        }

        public override void Show()
        {
            touchTimer = 0f;
            passTouches = false;
            touchReleaseTimer = 0f;
            move = vectZero;
            if (resetScrollOnShow)
            {
                SetScroll(vectZero);
            }
        }

        public override bool OnTouchDownXY(float tx, float ty)
        {
            if (!PointInRect(tx, ty, drawX, drawY, width, height))
            {
                return false;
            }
            if (touchPassTimeout == 0f)
            {
                bool flag = base.OnTouchDownXY(tx, ty);
                if (dontHandleTouchDownsHandledByChilds && flag)
                {
                    return true;
                }
            }
            else
            {
                touchTimer = touchPassTimeout;
                savedTouch = Vect(tx, ty);
                totalDrag = vectZero;
                passTouches = false;
            }
            touchState = TOUCH_STATE.DOWN;
            // movingByInertion = false;
            movingToSpoint = false;
            targetSpoint = -1;
            dragStart = Vect(tx, ty);
            return true;
        }

        public override bool OnTouchMoveXY(float tx, float ty)
        {
            if (touchPassTimeout == 0f || passTouches)
            {
                bool flag = base.OnTouchMoveXY(tx, ty);
                if (dontHandleTouchMovesHandledByChilds && flag)
                {
                    return true;
                }
            }
            Vector vector = Vect(tx, ty);
            if (VectEqual(dragStart, vector))
            {
                return false;
            }
            if (VectEqual(dragStart, impossibleTouch) && !PointInRect(tx, ty, drawX, drawY, width, height))
            {
                return false;
            }
            touchState = TOUCH_STATE.MOVING;
            if (!VectEqual(dragStart, impossibleTouch))
            {
                Vector vector2 = VectSub(vector, dragStart);
                dragStart = vector;
                vector2.X = FIT_TO_BOUNDARIES(vector2.X, 0f - maxTouchMoveLength, maxTouchMoveLength);
                vector2.Y = FIT_TO_BOUNDARIES(vector2.Y, 0f - maxTouchMoveLength, maxTouchMoveLength);
                totalDrag = VectAdd(totalDrag, vector2);
                if ((touchTimer > 0f || untouchChildsOnMove) && VectLength(totalDrag) > touchMoveIgnoreLength)
                {
                    touchTimer = 0f;
                    passTouches = false;
                    _ = base.OnTouchUpXY(-1f, -1f);
                }
                if (container.width <= width)
                {
                    vector2.X = 0f;
                }
                if (container.height <= height)
                {
                    vector2.Y = 0f;
                }
                if (shouldBounceHorizontally && (container.x > 0f || container.x < (-container.width + width)))
                {
                    vector2.X /= 2f;
                }
                if (shouldBounceVertically && (container.y > 0f || container.y < (-container.height + height)))
                {
                    vector2.Y /= 2f;
                }
                staticMove = MoveContainerBy(vector2);
                move = vectZero;
                inertiaTimeoutLeft = inertiaTimeout;
                return true;
            }
            return false;
        }

        public override bool OnTouchUpXY(float tx, float ty)
        {
            if (tx == -10000f && ty == -10000f)
            {
                return false;
            }
            if (touchPassTimeout == 0f || passTouches)
            {
                bool flag = base.OnTouchUpXY(tx, ty);
                if (dontHandleTouchUpsHandledByChilds && flag)
                {
                    return true;
                }
            }
            if (touchTimer > 0f)
            {
                bool flag2 = base.OnTouchDownXY(savedTouch.X, savedTouch.Y);
                touchReleaseTimer = 0.2f;
                touchTimer = 0f;
                if (dontHandleTouchDownsHandledByChilds && flag2)
                {
                    return true;
                }
            }
            if (touchState == TOUCH_STATE.UP)
            {
                return false;
            }
            touchState = TOUCH_STATE.UP;
            if (inertiaTimeoutLeft > 0f)
            {
                float inertiaRatio = inertiaTimeoutLeft / inertiaTimeout;
                move = VectMult(staticMove, inertiaRatio * 50f);
                // movingByInertion = true;
            }
            if (spointsNum > 0)
            {
                if (!canSkipScrollPoints)
                {
                    if (minAutoScrollToSpointLength != -1f && VectLength(move) > minAutoScrollToSpointLength)
                    {
                        StartMovingToSpointInDirection(move);
                    }
                    else
                    {
                        StartMovingToSpointInDirection(vectZero);
                    }
                }
                else if (VectEqual(move, vectZero))
                {
                    StartMovingToSpointInDirection(vectZero);
                }
            }
            dragStart = impossibleTouch;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                spoints = null;
            }
            base.Dispose(disposing);
        }

        public ScrollableContainer InitWithWidthHeightContainer(float w, float h, BaseElement c)
        {
            // float fixedDeltaSetting = ApplicationSettings.GetInt(5);
            // fixedDelta = (float)(1.0 / (double)fixedDeltaSetting);
            spoints = null;
            spointsNum = -1;
            spointsCapacity = -1;
            targetSpoint = -1;
            lastTargetSpoint = -1;
            // deaccelerationSpeed = 3f;
            inertiaTimeout = 0.1f;
            // scrollToPointDuration = 0.35f;
            canSkipScrollPoints = false;
            shouldBounceHorizontally = false;
            shouldBounceVertically = false;
            touchMoveIgnoreLength = 0f;
            maxTouchMoveLength = 40f;
            touchPassTimeout = 0.5f;
            minAutoScrollToSpointLength = -1f;
            resetScrollOnShow = true;
            untouchChildsOnMove = false;
            dontHandleTouchDownsHandledByChilds = false;
            dontHandleTouchMovesHandledByChilds = false;
            dontHandleTouchUpsHandledByChilds = false;
            touchTimer = 0f;
            passTouches = false;
            touchReleaseTimer = 0f;
            move = vectZero;
            container = c;
            width = (int)w;
            height = (int)h;
            container.parentAnchor = 9;
            container.parent = this;
            childs[0] = container;
            dragStart = impossibleTouch;
            touchState = TOUCH_STATE.UP;
            return this;
        }

        public ScrollableContainer InitWithWidthHeightContainerWidthHeight(float w, float h, float cw, float ch)
        {
            container = new BaseElement
            {
                width = (int)cw,
                height = (int)ch
            };
            _ = InitWithWidthHeightContainer(w, h, container);
            return this;
        }

        public void TurnScrollPointsOnWithCapacity(int n)
        {
            spointsCapacity = n;
            spoints = new Vector[spointsCapacity];
            spointsNum = 0;
        }

        public int AddScrollPointAtXY(float sx, float sy)
        {
            AddScrollPointAtXYwithID(sx, sy, spointsNum);
            return spointsNum - 1;
        }

        public void AddScrollPointAtXYwithID(float sx, float sy, int i)
        {
            spoints[i] = Vect(0f - sx, 0f - sy);
            if (i > spointsNum - 1)
            {
                spointsNum = i + 1;
            }
        }

        public int GetTotalScrollPoints()
        {
            return spointsNum;
        }

        public Vector GetScrollPoint(int i)
        {
            return spoints[i];
        }

        public Vector GetScroll()
        {
            return Vect(0f - container.x, 0f - container.y);
        }

        public Vector GetMaxScroll()
        {
            return Vect(container.width - width, container.height - height);
        }

        public void SetScroll(Vector s)
        {
            move = vectZero;
            container.x = 0f - s.X;
            container.y = 0f - s.Y;
            movingToSpoint = false;
            targetSpoint = -1;
            lastTargetSpoint = -1;
        }

        public void PlaceToScrollPoint(int sp)
        {
            move = vectZero;
            container.x = spoints[sp].X;
            container.y = spoints[sp].Y;
            movingToSpoint = false;
            targetSpoint = -1;
            lastTargetSpoint = sp;
            delegateScrollableContainerProtocol?.ScrollableContainerreachedScrollPoint(this, sp);
        }

        public void MoveToScrollPointmoveMultiplier(int sp, float m)
        {
            movingToSpoint = true;
            // movingByInertion = false;
            spointMoveMultiplier = m;
            targetSpoint = sp;
            lastTargetSpoint = targetSpoint;
        }

        public void CalculateNearsetScrollPointInDirection(Vector d)
        {
            // spointMoveDirection = d;
            int nearestScrollPoint = -1;
            float nearestDistance = 9999999f;
            float directionAngle = AngleTo0_360(RADIANS_TO_DEGREES(VectAngleNormalized(d)));
            Vector v = Vect(container.x, container.y);
            for (int i = 0; i < spointsNum; i++)
            {
                if (spoints[i].X <= 0f && (spoints[i].X >= (-container.width + width) || spoints[i].X >= 0f) && spoints[i].Y <= 0f && (spoints[i].Y >= (-container.height + height) || spoints[i].Y >= 0f))
                {
                    float candidateDistance = VectDistance(spoints[i], v);
                    if ((VectEqual(d, vectZero) || Math.Abs(AngleTo0_360(RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(spoints[i], v)))) - directionAngle) <= DEG_90) && candidateDistance < nearestDistance)
                    {
                        nearestScrollPoint = i;
                        nearestDistance = candidateDistance;
                    }
                }
            }
            if (nearestScrollPoint == -1 && !VectEqual(d, vectZero))
            {
                CalculateNearsetScrollPointInDirection(vectZero);
                return;
            }
            targetSpoint = nearestScrollPoint;
            if (!canSkipScrollPoints && targetSpoint != lastTargetSpoint)
            {
                //movingByInertion = false;
            }
            if (lastTargetSpoint != targetSpoint && targetSpoint != -1 && delegateScrollableContainerProtocol != null)
            {
                delegateScrollableContainerProtocol.ScrollableContainerchangedTargetScrollPoint(this, targetSpoint);
            }
            float moveAngle = AngleTo0_360(RADIANS_TO_DEGREES(VectAngleNormalized(move)));
            float targetAngle = AngleTo0_360(RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(spoints[targetSpoint], v))));
            spointMoveMultiplier = Math.Abs(AngleTo0_360(moveAngle - targetAngle)) < DEG_90 ? Math.Max(1f, VectLength(move) / 500f) : 0.5f;
            lastTargetSpoint = targetSpoint;
        }

        public Vector MoveContainerBy(Vector off)
        {
            float val = container.x + off.X;
            float val2 = container.y + off.Y;
            if (!shouldBounceHorizontally)
            {
                val = Math.Min(Math.Max(-container.width + width, val), 0f);
            }
            if (!shouldBounceVertically)
            {
                val2 = Math.Min(Math.Max(-container.height + height, val2), 0f);
            }
            Vector vector = VectSub(Vect(val, val2), Vect(container.x, container.y));
            container.x = val;
            container.y = val2;
            return vector;
        }

        public void MoveToPointDeltaSpeed(Vector tsp, float delta, float speed)
        {
            Vector v = VectSub(tsp, Vect(container.x, container.y));
            v = VectNormalize(v);
            v = VectMult(v, speed);
            _ = Mover.MoveVariableToTarget(ref container.x, tsp.X, Math.Abs(v.X), delta);
            _ = Mover.MoveVariableToTarget(ref container.y, tsp.Y, Math.Abs(v.Y), delta);
            targetPoint = tsp;
            move = vectZero;
        }

        public void StartMovingToSpointInDirection(Vector d)
        {
            movingToSpoint = true;
            targetSpoint = lastTargetSpoint = -1;
            CalculateNearsetScrollPointInDirection(d);
        }

        /// <remarks>
        /// This method provides smooth momentum-based scrolling with quick deceleration.
        /// <para>The scrolling behavior:</para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>Adds velocity to the existing momentum (allows smooth acceleration).</description>
        ///   </item>
        ///   <item>
        ///     <description>Velocity is automatically decelerated by the Update loop.</description>
        ///   </item>
        ///   <item>
        ///     <description>Scroll speed multiplier is <c>4f</c>.</description>
        ///   </item>
        ///   <item>
        ///     <description>No scrolling occurs if content height fits within the container.</description>
        ///   </item>
        /// </list>
        ///
        /// <para>Implementation notes:</para>
        /// <list type="bullet">
        ///   <item>
        ///     <description>The momentum system reuses existing drag/touch physics.</description>
        ///   </item>
        ///   <item>
        ///     <description>Deceleration factor (<c>7f</c>) balances smoothness and quick stopping.</description>
        ///   </item>
        ///   <item>
        ///     <description>Higher multiplier increases speed; higher deceleration stops faster.</description>
        ///   </item>
        /// </list>
        /// </remarks>
        public void HandleMouseWheel(int scrollDelta)
        {
            if (container.height <= height)
            {
                return; // No scrolling needed if content fits
            }

            // Convert scroll wheel delta to scroll velocity for smooth scrolling
            // Positive scrollDelta = scroll up (content moves down), negative = scroll down (content moves up)
            float scrollVelocity = scrollDelta * 4f;

            // Add to existing momentum for smooth, accumulating scrolling
            // The Update() method handles deceleration automatically
            move = VectAdd(move, Vect(0f, scrollVelocity));
        }

        public IScrollableContainerProtocol delegateScrollableContainerProtocol;

        private static readonly Vector impossibleTouch = new(-1000f, -1000f);

        private BaseElement container;

        private Vector dragStart;

        private Vector staticMove;

        private Vector move;

        // private bool movingByInertion;

        private float inertiaTimeoutLeft;

        private bool movingToSpoint;

        private int targetSpoint;

        private int lastTargetSpoint;

        private float spointMoveMultiplier;

        private Vector[] spoints;

        private int spointsNum;

        private int spointsCapacity;

        // private Vector spointMoveDirection;

        private Vector targetPoint;

        private TOUCH_STATE touchState;

        public float touchTimer;

        private float touchReleaseTimer;

        private Vector savedTouch;

        private Vector totalDrag;

        public bool passTouches;

        // private float fixedDelta;

        // private float deaccelerationSpeed;

        private float inertiaTimeout;

        // private float scrollToPointDuration;

        private bool canSkipScrollPoints;

        public bool shouldBounceHorizontally;

        public bool shouldBounceVertically;

        public float touchMoveIgnoreLength;

        private float maxTouchMoveLength;

        private float touchPassTimeout;

        public bool resetScrollOnShow;

        public bool dontHandleTouchDownsHandledByChilds;

        public bool dontHandleTouchMovesHandledByChilds;

        public bool dontHandleTouchUpsHandledByChilds;

        private bool untouchChildsOnMove;

        public float minAutoScrollToSpointLength;

        private enum TOUCH_STATE
        {
            UP,
            DOWN,
            MOVING
        }
    }
}
