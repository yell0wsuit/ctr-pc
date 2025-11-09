using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.windows;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.visual
{
    internal class ScrollableContainer : BaseElement
    {
        public void provideScrollPosMaxScrollPosScrollCoeff(ref Vector sp, ref Vector mp, ref Vector sc)
        {
            sp = getScroll();
            mp = getMaxScroll();
            float num = container.width / (float)width;
            float num2 = container.height / (float)height;
            sc = vect(num, num2);
        }

        public override int addChildwithID(BaseElement c, int i)
        {
            int num = container.addChildwithID(c, i);
            c.parentAnchor = 9;
            return num;
        }

        public override int addChild(BaseElement c)
        {
            c.parentAnchor = 9;
            return container.addChild(c);
        }

        public override void removeChildWithID(int i)
        {
            container.removeChildWithID(i);
        }

        public override void removeChild(BaseElement c)
        {
            container.removeChild(c);
        }

        public override BaseElement getChild(int i)
        {
            return container.getChild(i);
        }

        public override int childsCount()
        {
            return container.childsCount();
        }

        public override void draw()
        {
            float num = container.x;
            float num2 = container.y;
            container.x = (float)Math.Round(container.x);
            container.y = (float)Math.Round(container.y);
            base.preDraw();
            OpenGL.glEnable(4);
            OpenGL.setScissorRectangle(drawX, drawY, width, height);
            postDraw();
            OpenGL.glDisable(4);
            container.x = num;
            container.y = num2;
        }

        public override void postDraw()
        {
            if (!passTransformationsToChilds)
            {
                restoreTransformations(this);
            }
            container.preDraw();
            if (!container.passTransformationsToChilds)
            {
                restoreTransformations(container);
            }
            Dictionary<int, BaseElement> dictionary = container.getChilds();
            int i = 0;
            int count = dictionary.Count;
            while (i < count)
            {
                BaseElement baseElement = dictionary[i];
                float num = baseElement.drawX;
                float num2 = baseElement.drawY;
                if (baseElement != null && baseElement.visible && rectInRect(num, num2, num + baseElement.width, num2 + baseElement.height, drawX, drawY, drawX + width, drawY + height))
                {
                    baseElement.draw();
                }
                else
                {
                    calculateTopLeft(baseElement);
                }
                i++;
            }
            if (container.passTransformationsToChilds)
            {
                restoreTransformations(container);
            }
            if (passTransformationsToChilds)
            {
                restoreTransformations(this);
            }
        }

        public override void update(float delta)
        {
            base.update(delta);
            delta = fixedDelta;
            targetPoint = vectZero;
            if (touchTimer > 0.0)
            {
                touchTimer -= delta;
                if (touchTimer <= 0.0)
                {
                    touchTimer = 0f;
                    passTouches = true;
                    if (base.onTouchDownXY(savedTouch.x, savedTouch.y))
                    {
                        return;
                    }
                }
            }
            if (touchReleaseTimer > 0.0)
            {
                touchReleaseTimer -= delta;
                if (touchReleaseTimer <= 0.0)
                {
                    touchReleaseTimer = 0f;
                    if (base.onTouchUpXY(savedTouch.x, savedTouch.y))
                    {
                        return;
                    }
                }
            }
            if (touchState == TOUCH_STATE.TOUCH_STATE_UP)
            {
                if (shouldBounceHorizontally)
                {
                    if (container.x > 0.0)
                    {
                        float speed = (float)(50.0 + (double)Math.Abs(container.x) * 5.0);
                        moveToPointDeltaSpeed(vect(0f, container.y), delta, speed);
                    }
                    else if (container.x < (float)(-(float)container.width + width) && container.x < 0.0)
                    {
                        float speed2 = (float)(50.0 + (double)Math.Abs((float)(-(float)container.width + width) - container.x) * 5.0);
                        moveToPointDeltaSpeed(vect((float)(-(float)container.width + width), container.y), delta, speed2);
                    }
                }
                if (shouldBounceVertically)
                {
                    if (container.y > 0.0)
                    {
                        moveToPointDeltaSpeed(vect(container.x, 0f), delta, (float)(50.0 + (double)Math.Abs(container.y) * 5.0));
                    }
                    else if (container.y < (float)(-(float)container.height + height) && container.y < 0.0)
                    {
                        moveToPointDeltaSpeed(vect(container.x, (float)(-(float)container.height + height)), delta, (float)(50.0 + (double)Math.Abs((float)(-(float)container.height + height) - container.y) * 5.0));
                    }
                }
            }
            if (movingToSpoint)
            {
                Vector vector = spoints[targetSpoint];
                moveToPointDeltaSpeed(vector, delta, (float)Math.Max(100.0, (double)vectDistance(vector, vect(container.x, container.y)) * 4.0 * spointMoveMultiplier));
                if (container.x == vector.x && container.y == vector.y)
                {
                    delegateScrollableContainerProtocol?.scrollableContainerreachedScrollPoint(this, targetSpoint);
                    movingToSpoint = false;
                    targetSpoint = -1;
                    lastTargetSpoint = -1;
                    move = vectZero;
                }
            }
            else if (canSkipScrollPoints && spointsNum > 0 && !vectEqual(move, vectZero) && (double)vectLength(move) < 150.0 && targetSpoint == -1)
            {
                startMovingToSpointInDirection(move);
            }
            if (!vectEqual(move, vectZero))
            {
                vectEqual(targetPoint, vectZero);
                vect(container.x, container.y);
                Vector v = vectMult(vectNeg(move), 2f);
                move = vectAdd(move, vectMult(v, delta));
                Vector off = vectMult(move, delta);
                if ((double)Math.Abs(off.x) < 0.2)
                {
                    off.x = 0f;
                    move.x = 0f;
                }
                if ((double)Math.Abs(off.y) < 0.2)
                {
                    off.y = 0f;
                    move.y = 0f;
                }
                moveContainerBy(off);
            }
            if (inertiaTimeoutLeft > 0.0)
            {
                inertiaTimeoutLeft -= delta;
            }
        }

        public override void show()
        {
            touchTimer = 0f;
            passTouches = false;
            touchReleaseTimer = 0f;
            move = vectZero;
            if (resetScrollOnShow)
            {
                setScroll(vectZero);
            }
        }

        public override bool onTouchDownXY(float tx, float ty)
        {
            if (!pointInRect(tx, ty, drawX, drawY, width, height))
            {
                return false;
            }
            if (touchPassTimeout == 0f)
            {
                bool flag = base.onTouchDownXY(tx, ty);
                if (dontHandleTouchDownsHandledByChilds && flag)
                {
                    return true;
                }
            }
            else
            {
                touchTimer = touchPassTimeout;
                savedTouch = vect(tx, ty);
                totalDrag = vectZero;
                passTouches = false;
            }
            touchState = TOUCH_STATE.TOUCH_STATE_DOWN;
            movingByInertion = false;
            movingToSpoint = false;
            targetSpoint = -1;
            dragStart = vect(tx, ty);
            return true;
        }

        public override bool onTouchMoveXY(float tx, float ty)
        {
            if (touchPassTimeout == 0f || passTouches)
            {
                bool flag = base.onTouchMoveXY(tx, ty);
                if (dontHandleTouchMovesHandledByChilds && flag)
                {
                    return true;
                }
            }
            Vector vector = vect(tx, ty);
            if (vectEqual(dragStart, vector))
            {
                return false;
            }
            if (vectEqual(dragStart, impossibleTouch) && !pointInRect(tx, ty, drawX, drawY, width, height))
            {
                return false;
            }
            touchState = TOUCH_STATE.TOUCH_STATE_MOVING;
            if (!vectEqual(dragStart, impossibleTouch))
            {
                Vector vector2 = vectSub(vector, dragStart);
                dragStart = vector;
                vector2.x = FIT_TO_BOUNDARIES(vector2.x, 0f - maxTouchMoveLength, maxTouchMoveLength);
                vector2.y = FIT_TO_BOUNDARIES(vector2.y, 0f - maxTouchMoveLength, maxTouchMoveLength);
                totalDrag = vectAdd(totalDrag, vector2);
                if ((touchTimer > 0.0 || untouchChildsOnMove) && vectLength(totalDrag) > touchMoveIgnoreLength)
                {
                    touchTimer = 0f;
                    passTouches = false;
                    base.onTouchUpXY(-1f, -1f);
                }
                if (container.width <= width)
                {
                    vector2.x = 0f;
                }
                if (container.height <= height)
                {
                    vector2.y = 0f;
                }
                if (shouldBounceHorizontally && (container.x > 0.0 || container.x < (float)(-(float)container.width + width)))
                {
                    vector2.x /= 2f;
                }
                if (shouldBounceVertically && (container.y > 0.0 || container.y < (float)(-(float)container.height + height)))
                {
                    vector2.y /= 2f;
                }
                staticMove = moveContainerBy(vector2);
                move = vectZero;
                inertiaTimeoutLeft = inertiaTimeout;
                return true;
            }
            return false;
        }

        public override bool onTouchUpXY(float tx, float ty)
        {
            if (tx == -10000f && ty == -10000f)
            {
                return false;
            }
            if (touchPassTimeout == 0f || passTouches)
            {
                bool flag = base.onTouchUpXY(tx, ty);
                if (dontHandleTouchUpsHandledByChilds && flag)
                {
                    return true;
                }
            }
            if (touchTimer > 0.0)
            {
                bool flag2 = base.onTouchDownXY(savedTouch.x, savedTouch.y);
                touchReleaseTimer = 0.2f;
                touchTimer = 0f;
                if (dontHandleTouchDownsHandledByChilds && flag2)
                {
                    return true;
                }
            }
            if (touchState == TOUCH_STATE.TOUCH_STATE_UP)
            {
                return false;
            }
            touchState = TOUCH_STATE.TOUCH_STATE_UP;
            if (inertiaTimeoutLeft > 0.0)
            {
                float num = inertiaTimeoutLeft / inertiaTimeout;
                move = vectMult(staticMove, (float)((double)num * 50.0));
                movingByInertion = true;
            }
            if (spointsNum > 0)
            {
                if (!canSkipScrollPoints)
                {
                    if (minAutoScrollToSpointLength != -1f && vectLength(move) > minAutoScrollToSpointLength)
                    {
                        startMovingToSpointInDirection(move);
                    }
                    else
                    {
                        startMovingToSpointInDirection(vectZero);
                    }
                }
                else if (vectEqual(move, vectZero))
                {
                    startMovingToSpointInDirection(vectZero);
                }
            }
            dragStart = impossibleTouch;
            return true;
        }

        public override void dealloc()
        {
            spoints = null;
            base.dealloc();
        }

        public virtual ScrollableContainer initWithWidthHeightContainer(float w, float h, BaseElement c)
        {
            if (init() != null)
            {
                float num = Application.sharedAppSettings().getInt(5);
                fixedDelta = (float)(1.0 / (double)num);
                spoints = null;
                spointsNum = -1;
                spointsCapacity = -1;
                targetSpoint = -1;
                lastTargetSpoint = -1;
                deaccelerationSpeed = 3f;
                inertiaTimeout = 0.1f;
                scrollToPointDuration = 0.35f;
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
                touchState = TOUCH_STATE.TOUCH_STATE_UP;
            }
            return this;
        }

        public virtual ScrollableContainer initWithWidthHeightContainerWidthHeight(float w, float h, float cw, float ch)
        {
            container = (BaseElement)new BaseElement().init();
            container.width = (int)cw;
            container.height = (int)ch;
            initWithWidthHeightContainer(w, h, container);
            return this;
        }

        public virtual void turnScrollPointsOnWithCapacity(int n)
        {
            spointsCapacity = n;
            spoints = new Vector[spointsCapacity];
            spointsNum = 0;
        }

        public virtual int addScrollPointAtXY(double sx, double sy)
        {
            return addScrollPointAtXY((float)sx, (float)sy);
        }

        public virtual int addScrollPointAtXY(float sx, float sy)
        {
            addScrollPointAtXYwithID(sx, sy, spointsNum);
            return spointsNum - 1;
        }

        public virtual void addScrollPointAtXYwithID(float sx, float sy, int i)
        {
            spoints[i] = vect(0f - sx, 0f - sy);
            if (i > spointsNum - 1)
            {
                spointsNum = i + 1;
            }
        }

        public virtual int getTotalScrollPoints()
        {
            return spointsNum;
        }

        public virtual Vector getScrollPoint(int i)
        {
            return spoints[i];
        }

        public virtual Vector getScroll()
        {
            return vect(0f - container.x, 0f - container.y);
        }

        public virtual Vector getMaxScroll()
        {
            return vect(container.width - width, container.height - height);
        }

        public virtual void setScroll(Vector s)
        {
            move = vectZero;
            container.x = 0f - s.x;
            container.y = 0f - s.y;
            movingToSpoint = false;
            targetSpoint = -1;
            lastTargetSpoint = -1;
        }

        public virtual void placeToScrollPoint(int sp)
        {
            move = vectZero;
            container.x = spoints[sp].x;
            container.y = spoints[sp].y;
            movingToSpoint = false;
            targetSpoint = -1;
            lastTargetSpoint = sp;
            delegateScrollableContainerProtocol?.scrollableContainerreachedScrollPoint(this, sp);
        }

        public virtual void moveToScrollPointmoveMultiplier(int sp, double m)
        {
            moveToScrollPointmoveMultiplier(sp, (float)m);
        }

        public virtual void moveToScrollPointmoveMultiplier(int sp, float m)
        {
            movingToSpoint = true;
            movingByInertion = false;
            spointMoveMultiplier = m;
            targetSpoint = sp;
            lastTargetSpoint = targetSpoint;
        }

        public virtual void calculateNearsetScrollPointInDirection(Vector d)
        {
            spointMoveDirection = d;
            int num = -1;
            float num2 = 9999999f;
            float num3 = angleTo0_360(RADIANS_TO_DEGREES(vectAngleNormalized(d)));
            Vector v = vect(container.x, container.y);
            for (int i = 0; i < spointsNum; i++)
            {
                if (spoints[i].x <= 0.0 && (spoints[i].x >= (float)(-(float)container.width + width) || spoints[i].x >= 0.0) && spoints[i].y <= 0.0 && (spoints[i].y >= (float)(-(float)container.height + height) || spoints[i].y >= 0.0))
                {
                    float num4 = vectDistance(spoints[i], v);
                    if ((vectEqual(d, vectZero) || Math.Abs(angleTo0_360(RADIANS_TO_DEGREES(vectAngleNormalized(vectSub(spoints[i], v)))) - num3) <= 90f) && num4 < num2)
                    {
                        num = i;
                        num2 = num4;
                    }
                }
            }
            if (num == -1 && !vectEqual(d, vectZero))
            {
                calculateNearsetScrollPointInDirection(vectZero);
                return;
            }
            targetSpoint = num;
            if (!canSkipScrollPoints && targetSpoint != lastTargetSpoint)
            {
                movingByInertion = false;
            }
            if (lastTargetSpoint != targetSpoint && targetSpoint != -1 && delegateScrollableContainerProtocol != null)
            {
                delegateScrollableContainerProtocol.scrollableContainerchangedTargetScrollPoint(this, targetSpoint);
            }
            float num6 = angleTo0_360(RADIANS_TO_DEGREES(vectAngleNormalized(move)));
            float num5 = angleTo0_360(RADIANS_TO_DEGREES(vectAngleNormalized(vectSub(spoints[targetSpoint], v))));
            if (Math.Abs(angleTo0_360(num6 - num5)) < 90f)
            {
                spointMoveMultiplier = (float)Math.Max(1.0, (double)vectLength(move) / 500.0);
            }
            else
            {
                spointMoveMultiplier = 0.5f;
            }
            lastTargetSpoint = targetSpoint;
        }

        public virtual Vector moveContainerBy(Vector off)
        {
            float val = container.x + off.x;
            float val2 = container.y + off.y;
            if (!shouldBounceHorizontally)
            {
                val = (float)Math.Min((double)Math.Max((float)(-(float)container.width + width), val), 0.0);
            }
            if (!shouldBounceVertically)
            {
                val2 = (float)Math.Min((double)Math.Max((float)(-(float)container.height + height), val2), 0.0);
            }
            Vector vector = vectSub(vect(val, val2), vect(container.x, container.y));
            container.x = val;
            container.y = val2;
            return vector;
        }

        public virtual void moveToPointDeltaSpeed(Vector tsp, float delta, float speed)
        {
            Vector v = vectSub(tsp, vect(container.x, container.y));
            v = vectNormalize(v);
            v = vectMult(v, speed);
            Mover.moveVariableToTarget(ref container.x, tsp.x, Math.Abs(v.x), delta);
            Mover.moveVariableToTarget(ref container.y, tsp.y, Math.Abs(v.y), delta);
            targetPoint = tsp;
            move = vectZero;
        }

        public virtual void startMovingToSpointInDirection(Vector d)
        {
            movingToSpoint = true;
            targetSpoint = lastTargetSpoint = -1;
            calculateNearsetScrollPointInDirection(d);
        }

        private const double DEFAULT_BOUNCE_MOVEMENT_DIVIDE = 2.0;

        private const double DEFAULT_BOUNCE_DURATION = 0.1;

        private const double DEFAULT_DEACCELERATION = 3.0;

        private const double DEFAULT_INERTIAL_TIMEOUT = 0.1;

        private const double DEFAULT_SCROLL_TO_POINT_DURATION = 0.35;

        private const double MIN_SCROLL_POINTS_MOVE = 50.0;

        private const double CALC_NEAREST_DEFAULT_TIMEOUT = 0.02;

        private const double DEFAULT_MAX_TOUCH_MOVE_LENGTH = 40.0;

        private const double DEFAULT_TOUCH_PASS_TIMEOUT = 0.5;

        private const double AUTO_RELEASE_TOUCH_TIMEOUT = 0.2;

        private const double MOVE_APPROXIMATION = 0.2;

        public ScrollableContainerProtocol delegateScrollableContainerProtocol;

        private static readonly Vector impossibleTouch = new(-1000f, -1000f);

        private BaseElement container;

        private Vector dragStart;

        private Vector staticMove;

        private Vector move;

        private bool movingByInertion;

        private float inertiaTimeoutLeft;

        private bool movingToSpoint;

        private int targetSpoint;

        private int lastTargetSpoint;

        private float spointMoveMultiplier;

        private Vector[] spoints;

        private int spointsNum;

        private int spointsCapacity;

        private Vector spointMoveDirection;

        private Vector targetPoint;

        private TOUCH_STATE touchState;

        public float touchTimer;

        private float touchReleaseTimer;

        private Vector savedTouch;

        private Vector totalDrag;

        public bool passTouches;

        private float fixedDelta;

        private float deaccelerationSpeed;

        private float inertiaTimeout;

        private float scrollToPointDuration;

        private bool canSkipScrollPoints;

        public bool shouldBounceHorizontally;

        private bool shouldBounceVertically;

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
            TOUCH_STATE_UP,
            TOUCH_STATE_DOWN,
            TOUCH_STATE_MOVING
        }
    }
}
