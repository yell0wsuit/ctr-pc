using System;
using System.Collections.Generic;

using CutTheRope.Desktop;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Helpers;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> that provides scrollable, clipped content with touch-based drag, inertia, bounce, and scroll-point snapping.
    /// </summary>
    internal sealed class ScrollableContainer : BaseElement
    {
        /// <summary>
        /// Outputs the current scroll position, maximum scroll position, and scroll coefficients.
        /// </summary>
        /// <param name="sp">Receives the current scroll position.</param>
        /// <param name="mp">Receives the maximum scroll position.</param>
        /// <param name="sc">Receives the scroll coefficients (container/viewport ratio).</param>
        public void ProvideScrollPosMaxScrollPosScrollCoeff(ref Vector sp, ref Vector mp, ref Vector sc)
        {
            sp = GetScroll();
            mp = GetMaxScroll();
            float scrollCoeffX = container.width / width;
            float scrollCoeffY = container.height / height;
            sc = Vect(scrollCoeffX, scrollCoeffY);
        }

        /// <inheritdoc />
        public override int AddChildwithID(BaseElement c, int i)
        {
            int childId = container.AddChildwithID(c, i);
            c.parentAnchor = 9;
            return childId;
        }

        /// <inheritdoc />
        public override int AddChild(BaseElement c)
        {
            c.parentAnchor = 9;
            return container.AddChild(c);
        }

        /// <inheritdoc />
        public override void RemoveChildWithID(int i)
        {
            container.RemoveChildWithID(i);
        }

        /// <inheritdoc />
        public override void RemoveChild(BaseElement c)
        {
            container.RemoveChild(c);
        }

        /// <inheritdoc />
        public override BaseElement GetChild(int i)
        {
            return container.GetChild(i);
        }

        /// <inheritdoc />
        public override int ChildsCount()
        {
            return container.ChildsCount();
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            Renderer.Enable(Renderer.GL_SCISSOR_TEST);
            Renderer.SetScissor(drawX, drawY, width, height);
            PostDraw();
            Renderer.Disable(Renderer.GL_SCISSOR_TEST);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
                        float speed = 50 + (MathF.Abs(container.x) * 5);
                        MoveToPointDeltaSpeed(Vect(0f, container.y), delta, speed);
                    }
                    else if (container.x < (-container.width + width) && container.x < 0)
                    {
                        float speed2 = 50 + (MathF.Abs(-container.width + width - container.x) * 5);
                        MoveToPointDeltaSpeed(Vect(-container.width + width, container.y), delta, speed2);
                    }
                }
                if (shouldBounceVertically)
                {
                    if (container.y > 0)
                    {
                        MoveToPointDeltaSpeed(Vect(container.x, 0f), delta, 50f + (MathF.Abs(container.y) * 5f));
                    }
                    else if (container.y < (-container.height + height) && container.y < 0f)
                    {
                        MoveToPointDeltaSpeed(Vect(container.x, -container.height + height), delta, 50f + (MathF.Abs(-container.height + height - container.y) * 5f));
                    }
                }
            }
            if (movingToSpoint)
            {
                Vector vector = spoints[targetSpoint];
                MoveToPointDeltaSpeed(vector, delta, MathF.Max(100f, VectDistance(vector, Vect(container.x, container.y)) * 4f * spointMoveMultiplier));
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
                if (MathF.Abs(off.X) < 0.2f)
                {
                    off.X = 0f;
                    move.X = 0f;
                }
                if (MathF.Abs(off.Y) < 0.2f)
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                spoints = null;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Initializes the container with the specified viewport size and content element.
        /// Resets scroll-point state, touch handling, and scrolling behavior to defaults.
        /// </summary>
        /// <param name="w">Viewport width.</param>
        /// <param name="h">Viewport height.</param>
        /// <param name="c">Content element that will be clipped and scrolled inside the viewport.</param>
        /// <returns>The initialized container instance.</returns>
        public ScrollableContainer InitWithWidthHeightContainer(float w, float h, BaseElement c)
        {
            // float fixedDeltaSetting = ApplicationSettings.GetInt(5);
            // fixedDelta = (1 / fixedDeltaSetting);
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

        /// <summary>
        /// Initializes the container with an empty content element sized to the specified content bounds.
        /// </summary>
        /// <param name="w">Viewport width.</param>
        /// <param name="h">Viewport height.</param>
        /// <param name="cw">Scrollable content width.</param>
        /// <param name="ch">Scrollable content height.</param>
        /// <returns>The initialized container instance.</returns>
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

        /// <summary>
        /// Enables scroll-point support and allocates storage for up to <paramref name="n"/> points.
        /// </summary>
        /// <param name="n">Maximum number of scroll points to store.</param>
        public void TurnScrollPointsOnWithCapacity(int n)
        {
            spointsCapacity = n;
            spoints = new Vector[spointsCapacity];
            spointsNum = 0;
        }

        /// <summary>
        /// Adds a scroll point using logical scroll coordinates.
        /// </summary>
        /// <param name="sx">Horizontal scroll position associated with the point.</param>
        /// <param name="sy">Vertical scroll position associated with the point.</param>
        /// <returns>The index assigned to the new scroll point.</returns>
        public int AddScrollPointAtXY(float sx, float sy)
        {
            AddScrollPointAtXYwithID(sx, sy, spointsNum);
            return spointsNum - 1;
        }

        /// <summary>
        /// Stores a scroll point at the specified index.
        /// </summary>
        /// <param name="sx">Horizontal scroll position associated with the point.</param>
        /// <param name="sy">Vertical scroll position associated with the point.</param>
        /// <param name="i">Target scroll point index.</param>
        public void AddScrollPointAtXYwithID(float sx, float sy, int i)
        {
            spoints[i] = Vect(0f - sx, 0f - sy);
            if (i > spointsNum - 1)
            {
                spointsNum = i + 1;
            }
        }

        /// <summary>
        /// Returns the number of registered scroll points.
        /// </summary>
        /// <returns>Total registered scroll points.</returns>
        public int GetTotalScrollPoints()
        {
            return spointsNum;
        }

        /// <summary>
        /// Returns the stored container offset for the specified scroll point.
        /// </summary>
        /// <param name="i">Scroll point index.</param>
        /// <returns>Container offset used when snapping to the scroll point.</returns>
        public Vector GetScrollPoint(int i)
        {
            return spoints[i];
        }

        /// <summary>
        /// Returns the current logical scroll offset.
        /// </summary>
        /// <returns>Current horizontal and vertical scroll offset.</returns>
        public Vector GetScroll()
        {
            return Vect(0f - container.x, 0f - container.y);
        }

        /// <summary>
        /// Returns the maximum logical scroll offset allowed by the current content size.
        /// </summary>
        /// <returns>Maximum horizontal and vertical scroll offset.</returns>
        public Vector GetMaxScroll()
        {
            return Vect(container.width - width, container.height - height);
        }

        /// <summary>
        /// Sets the current scroll offset immediately and cancels inertial or snap-to-point movement.
        /// </summary>
        /// <param name="s">Logical scroll offset to apply.</param>
        public void SetScroll(Vector s)
        {
            move = vectZero;
            container.x = 0f - s.X;
            container.y = 0f - s.Y;
            movingToSpoint = false;
            targetSpoint = -1;
            lastTargetSpoint = -1;
        }

        /// <summary>
        /// Jumps directly to the specified scroll point and notifies the delegate that it was reached.
        /// </summary>
        /// <param name="sp">Scroll point index.</param>
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

        /// <summary>
        /// Starts animating toward the specified scroll point.
        /// </summary>
        /// <param name="sp">Target scroll point index.</param>
        /// <param name="m">Speed multiplier applied while moving toward the point.</param>
        public void MoveToScrollPointmoveMultiplier(int sp, float m)
        {
            movingToSpoint = true;
            // movingByInertion = false;
            spointMoveMultiplier = m;
            targetSpoint = sp;
            lastTargetSpoint = targetSpoint;
        }

        /// <summary>
        /// Selects the nearest valid scroll point, preferring points that lie in the supplied direction.
        /// Updates the target scroll-point state and delegate notifications.
        /// </summary>
        /// <param name="d">Preferred movement direction. Use <c>vectZero</c> to ignore direction.</param>
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
                    if ((VectEqual(d, vectZero) || MathF.Abs(AngleTo0_360(RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(spoints[i], v)))) - directionAngle) <= DEG_90) && candidateDistance < nearestDistance)
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
            spointMoveMultiplier = MathF.Abs(AngleTo0_360(moveAngle - targetAngle)) < DEG_90 ? MathF.Max(1f, VectLength(move) / 500f) : 0.5f;
            lastTargetSpoint = targetSpoint;
        }

        /// <summary>
        /// Moves the content container by the requested offset, clamping to bounds unless bounce is enabled.
        /// </summary>
        /// <param name="off">Requested movement offset in container space.</param>
        /// <returns>The actual applied movement after bounds checks.</returns>
        public Vector MoveContainerBy(Vector off)
        {
            float val = container.x + off.X;
            float val2 = container.y + off.Y;
            if (!shouldBounceHorizontally)
            {
                val = MathF.Min(MathF.Max(-container.width + width, val), 0f);
            }
            if (!shouldBounceVertically)
            {
                val2 = MathF.Min(MathF.Max(-container.height + height, val2), 0f);
            }
            Vector vector = VectSub(Vect(val, val2), Vect(container.x, container.y));
            container.x = val;
            container.y = val2;
            return vector;
        }

        /// <summary>
        /// Advances the content container toward a target position at the provided <paramref name="speed"/> for the current frame.
        /// </summary>
        /// <param name="tsp">Target position in container space.</param>
        /// <param name="delta">Elapsed frame time in seconds.</param>
        /// <param name="speed">Movement speed applied toward the target.</param>
        public void MoveToPointDeltaSpeed(Vector tsp, float delta, float speed)
        {
            Vector v = VectSub(tsp, Vect(container.x, container.y));
            v = VectNormalize(v);
            v = VectMult(v, speed);
            _ = Mover.MoveVariableToTarget(ref container.x, tsp.X, MathF.Abs(v.X), delta);
            _ = Mover.MoveVariableToTarget(ref container.y, tsp.Y, MathF.Abs(v.Y), delta);
            targetPoint = tsp;
            move = vectZero;
        }

        /// <summary>
        /// Starts snap-to-point movement by selecting the nearest scroll point in the supplied direction.
        /// </summary>
        /// <param name="d">Preferred movement direction used to choose a scroll point.</param>
        public void StartMovingToSpointInDirection(Vector d)
        {
            movingToSpoint = true;
            targetSpoint = lastTargetSpoint = -1;
            CalculateNearsetScrollPointInDirection(d);
        }

        /// <summary>
        /// Provides smooth, momentum-based scrolling in response to mouse wheel input.
        /// </summary>
        /// <param name="scrollDelta">
        /// The mouse wheel delta value. Positive values scroll up (content moves down),
        /// negative values scroll down (content moves up).
        /// </param>
        public void HandleMouseWheel(int scrollDelta)
        {
            if (container.height <= height)
            {
                return; // No scrolling needed if content fits
            }

            // Convert scroll wheel delta to scroll velocity for smooth scrolling
            float scrollVelocity = scrollDelta * 4f;

            // Add to existing momentum for smooth, accumulating scrolling
            // The Update() method handles deceleration automatically
            move = VectAdd(move, Vect(0f, scrollVelocity));
        }

        /// <summary>
        /// Receives notifications when the active target or reached scroll point changes.
        /// </summary>
        public IScrollableContainerProtocol delegateScrollableContainerProtocol;

        /// <summary>
        /// Sentinel touch position used to mark the absence of an active drag gesture.
        /// </summary>
        private static readonly Vector impossibleTouch = new(-1000f, -1000f);

        /// <summary>
        /// Root content element that is clipped and translated during scrolling.
        /// </summary>
        private BaseElement container;

        /// <summary>
        /// Last touch position used to measure incremental drag movement.
        /// </summary>
        private Vector dragStart;

        /// <summary>
        /// Most recent drag displacement, reused to seed inertial scrolling on release.
        /// </summary>
        private Vector staticMove;

        /// <summary>
        /// Current scrolling velocity used for inertia and mouse-wheel momentum.
        /// </summary>
        private Vector move;

        // private bool movingByInertion;

        /// <summary>
        /// Remaining time window during which drag motion can be converted into inertial movement.
        /// </summary>
        private float inertiaTimeoutLeft;

        /// <summary>
        /// Whether the container is currently animating toward a snap point.
        /// </summary>
        private bool movingToSpoint;

        /// <summary>
        /// Index of the current snap-point target, or <c>-1</c> when no target is active.
        /// </summary>
        private int targetSpoint;

        /// <summary>
        /// Index of the previously selected snap point.
        /// </summary>
        private int lastTargetSpoint;

        /// <summary>
        /// Speed multiplier applied while animating toward the selected snap point.
        /// </summary>
        private float spointMoveMultiplier;

        /// <summary>
        /// Stored snap-point container offsets.
        /// </summary>
        private Vector[] spoints;

        /// <summary>
        /// Number of snap points currently registered in <see cref="spoints"/>.
        /// </summary>
        private int spointsNum;

        /// <summary>
        /// Allocated capacity of the snap-point storage array.
        /// </summary>
        private int spointsCapacity;

        // private Vector spointMoveDirection;

        /// <summary>
        /// Most recent explicit movement target used by helper movement routines.
        /// </summary>
        private Vector targetPoint;

        /// <summary>
        /// Current touch interaction phase for the container.
        /// </summary>
        private TOUCH_STATE touchState;

        /// <summary>
        /// Remaining delay before touch input is forwarded to child elements.
        /// </summary>
        public float touchTimer;

        /// <summary>
        /// Delay before a deferred child touch-up event is replayed.
        /// </summary>
        private float touchReleaseTimer;

        /// <summary>
        /// Cached touch position used when touch delivery to children is deferred.
        /// </summary>
        private Vector savedTouch;

        /// <summary>
        /// Total drag distance accumulated during the current gesture.
        /// </summary>
        private Vector totalDrag;

        /// <summary>
        /// Whether touch events should currently be forwarded to child elements.
        /// </summary>
        public bool passTouches;

        // private float fixedDelta;

        // private float deaccelerationSpeed;

        /// <summary>
        /// Maximum time after a drag movement during which inertia can still be applied on release.
        /// </summary>
        private float inertiaTimeout;

        // private float scrollToPointDuration;

        /// <summary>
        /// Whether snap selection may skip intermediate scroll points instead of moving point-by-point.
        /// </summary>
        private bool canSkipScrollPoints;

        /// <summary>
        /// Whether horizontal overscroll is allowed and springs back after release.
        /// </summary>
        public bool shouldBounceHorizontally;

        /// <summary>
        /// Whether vertical overscroll is allowed and springs back after release.
        /// </summary>
        public bool shouldBounceVertically;

        /// <summary>
        /// Drag distance required before pending child touches are cancelled and scrolling takes control.
        /// </summary>
        public float touchMoveIgnoreLength;

        /// <summary>
        /// Maximum per-frame drag delta applied from touch movement before clamping.
        /// </summary>
        private float maxTouchMoveLength;

        /// <summary>
        /// Delay before touch events are passed through to child elements.
        /// </summary>
        private float touchPassTimeout;

        /// <summary>
        /// Whether <see cref="Show"/> resets the scroll offset to the origin.
        /// </summary>
        public bool resetScrollOnShow;

        /// <summary>
        /// Whether the container stops processing touch-down events already handled by a child.
        /// </summary>
        public bool dontHandleTouchDownsHandledByChilds;

        /// <summary>
        /// Whether the container stops processing touch-move events already handled by a child.
        /// </summary>
        public bool dontHandleTouchMovesHandledByChilds;

        /// <summary>
        /// Whether the container stops processing touch-up events already handled by a child.
        /// </summary>
        public bool dontHandleTouchUpsHandledByChilds;

        /// <summary>
        /// Whether dragging should explicitly cancel touches already delivered to children.
        /// </summary>
        private bool untouchChildsOnMove;

        /// <summary>
        /// Minimum release movement length required to choose a snap target from release direction.
        /// Use <c>-1</c> to disable the directional threshold.
        /// </summary>
        public float minAutoScrollToSpointLength;

        /// <summary>
        /// Touch interaction states used to track dragging lifecycle.
        /// </summary>
        private enum TOUCH_STATE
        {
            /// <summary>
            /// No active touch is being tracked.
            /// </summary>
            UP,

            /// <summary>
            /// A touch has started but has not yet moved.
            /// </summary>
            DOWN,

            /// <summary>
            /// An active touch is dragging the container.
            /// </summary>
            MOVING
        }
    }
}
