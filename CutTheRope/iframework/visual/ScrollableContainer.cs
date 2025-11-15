using System;
using System.Collections.Generic;

using CutTheRope.desktop;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;

namespace CutTheRope.iframework.visual
{
    internal sealed class ScrollableContainer : BaseElement
    {
        public void ProvideScrollPosMaxScrollPosScrollCoeff(ref Vector sp, ref Vector mp, ref Vector sc)
        {
            sp = GetScroll();
            mp = GetMaxScroll();
            float num = container.width / (float)width;
            float num2 = container.height / (float)height;
            sc = Vect(num, num2);
        }

        public override int AddChildwithID(BaseElement c, int i)
        {
            int num = container.AddChildwithID(c, i);
            c.parentAnchor = 9;
            return num;
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
            float num = container.x;
            float num2 = container.y;
            container.x = (float)Math.Round(container.x);
            container.y = (float)Math.Round(container.y);
            PreDraw();
            OpenGL.GlEnable(4);
            OpenGL.SetScissorRectangle(drawX, drawY, width, height);
            PostDraw();
            OpenGL.GlDisable(4);
            container.x = num;
            container.y = num2;
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
                float num = baseElement.drawX;
                float num2 = baseElement.drawY;
                if (baseElement != null && baseElement.visible && RectInRect(num, num2, num + baseElement.width, num2 + baseElement.height, drawX, drawY, drawX + width, drawY + height))
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
            delta = fixedDelta;
            targetPoint = vectZero;
            if (touchTimer > 0.0)
            {
                touchTimer -= delta;
                if (touchTimer <= 0.0)
                {
                    touchTimer = 0f;
                    passTouches = true;
                    if (base.OnTouchDownXY(savedTouch.x, savedTouch.y))
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
                    if (base.OnTouchUpXY(savedTouch.x, savedTouch.y))
                    {
                        return;
                    }
                }
            }
            if (touchState == TOUCH_STATE.UP)
            {
                if (shouldBounceHorizontally)
                {
                    if (container.x > 0.0)
                    {
                        float speed = (float)(50.0 + ((double)Math.Abs(container.x) * 5.0));
                        MoveToPointDeltaSpeed(Vect(0f, container.y), delta, speed);
                    }
                    else if (container.x < (float)(-(float)container.width + width) && container.x < 0.0)
                    {
                        float speed2 = (float)(50.0 + ((double)Math.Abs((float)(-(float)container.width + width) - container.x) * 5.0));
                        MoveToPointDeltaSpeed(Vect((float)(-(float)container.width + width), container.y), delta, speed2);
                    }
                }
                if (shouldBounceVertically)
                {
                    if (container.y > 0.0)
                    {
                        MoveToPointDeltaSpeed(Vect(container.x, 0f), delta, (float)(50.0 + ((double)Math.Abs(container.y) * 5.0)));
                    }
                    else if (container.y < (float)(-(float)container.height + height) && container.y < 0.0)
                    {
                        MoveToPointDeltaSpeed(Vect(container.x, (float)(-(float)container.height + height)), delta, (float)(50.0 + ((double)Math.Abs((float)(-(float)container.height + height) - container.y) * 5.0)));
                    }
                }
            }
            if (movingToSpoint)
            {
                Vector vector = spoints[targetSpoint];
                MoveToPointDeltaSpeed(vector, delta, (float)Math.Max(100.0, (double)VectDistance(vector, Vect(container.x, container.y)) * 4.0 * spointMoveMultiplier));
                if (container.x == vector.x && container.y == vector.y)
                {
                    delegateScrollableContainerProtocol?.ScrollableContainerreachedScrollPoint(this, targetSpoint);
                    movingToSpoint = false;
                    targetSpoint = -1;
                    lastTargetSpoint = -1;
                    move = vectZero;
                }
            }
            else if (canSkipScrollPoints && spointsNum > 0 && !VectEqual(move, vectZero) && (double)VectLength(move) < 150.0 && targetSpoint == -1)
            {
                StartMovingToSpointInDirection(move);
            }
            if (!VectEqual(move, vectZero))
            {
                _ = VectEqual(targetPoint, vectZero);
                _ = Vect(container.x, container.y);
                Vector v = VectMult(VectNeg(move), 2f);
                move = VectAdd(move, VectMult(v, delta));
                Vector off = VectMult(move, delta);
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
                _ = MoveContainerBy(off);
            }
            if (inertiaTimeoutLeft > 0.0)
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
                vector2.x = FIT_TO_BOUNDARIES(vector2.x, 0f - maxTouchMoveLength, maxTouchMoveLength);
                vector2.y = FIT_TO_BOUNDARIES(vector2.y, 0f - maxTouchMoveLength, maxTouchMoveLength);
                totalDrag = VectAdd(totalDrag, vector2);
                if ((touchTimer > 0.0 || untouchChildsOnMove) && VectLength(totalDrag) > touchMoveIgnoreLength)
                {
                    touchTimer = 0f;
                    passTouches = false;
                    _ = base.OnTouchUpXY(-1f, -1f);
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
            if (touchTimer > 0.0)
            {
                bool flag2 = base.OnTouchDownXY(savedTouch.x, savedTouch.y);
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
            if (inertiaTimeoutLeft > 0.0)
            {
                float num = inertiaTimeoutLeft / inertiaTimeout;
                move = VectMult(staticMove, (float)((double)num * 50.0));
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
            float num = Application.SharedAppSettings().GetInt(5);
            fixedDelta = (float)(1.0 / (double)num);
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

        public int AddScrollPointAtXY(double sx, double sy)
        {
            return AddScrollPointAtXY((float)sx, (float)sy);
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
            container.x = 0f - s.x;
            container.y = 0f - s.y;
            movingToSpoint = false;
            targetSpoint = -1;
            lastTargetSpoint = -1;
        }

        public void PlaceToScrollPoint(int sp)
        {
            move = vectZero;
            container.x = spoints[sp].x;
            container.y = spoints[sp].y;
            movingToSpoint = false;
            targetSpoint = -1;
            lastTargetSpoint = sp;
            delegateScrollableContainerProtocol?.ScrollableContainerreachedScrollPoint(this, sp);
        }

        public void MoveToScrollPointmoveMultiplier(int sp, double m)
        {
            MoveToScrollPointmoveMultiplier(sp, (float)m);
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
            int num = -1;
            float num2 = 9999999f;
            float num3 = AngleTo0_360(RADIANS_TO_DEGREES(VectAngleNormalized(d)));
            Vector v = Vect(container.x, container.y);
            for (int i = 0; i < spointsNum; i++)
            {
                if (spoints[i].x <= 0.0 && (spoints[i].x >= (float)(-(float)container.width + width) || spoints[i].x >= 0.0) && spoints[i].y <= 0.0 && (spoints[i].y >= (float)(-(float)container.height + height) || spoints[i].y >= 0.0))
                {
                    float num4 = VectDistance(spoints[i], v);
                    if ((VectEqual(d, vectZero) || Math.Abs(AngleTo0_360(RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(spoints[i], v)))) - num3) <= 90f) && num4 < num2)
                    {
                        num = i;
                        num2 = num4;
                    }
                }
            }
            if (num == -1 && !VectEqual(d, vectZero))
            {
                CalculateNearsetScrollPointInDirection(vectZero);
                return;
            }
            targetSpoint = num;
            if (!canSkipScrollPoints && targetSpoint != lastTargetSpoint)
            {
                //movingByInertion = false;
            }
            if (lastTargetSpoint != targetSpoint && targetSpoint != -1 && delegateScrollableContainerProtocol != null)
            {
                delegateScrollableContainerProtocol.ScrollableContainerchangedTargetScrollPoint(this, targetSpoint);
            }
            float num6 = AngleTo0_360(RADIANS_TO_DEGREES(VectAngleNormalized(move)));
            float num5 = AngleTo0_360(RADIANS_TO_DEGREES(VectAngleNormalized(VectSub(spoints[targetSpoint], v))));
            spointMoveMultiplier = Math.Abs(AngleTo0_360(num6 - num5)) < 90f ? (float)Math.Max(1.0, (double)VectLength(move) / 500.0) : 0.5f;
            lastTargetSpoint = targetSpoint;
        }

        public Vector MoveContainerBy(Vector off)
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
            _ = Mover.MoveVariableToTarget(ref container.x, tsp.x, Math.Abs(v.x), delta);
            _ = Mover.MoveVariableToTarget(ref container.y, tsp.y, Math.Abs(v.y), delta);
            targetPoint = tsp;
            move = vectZero;
        }

        public void StartMovingToSpointInDirection(Vector d)
        {
            movingToSpoint = true;
            targetSpoint = lastTargetSpoint = -1;
            CalculateNearsetScrollPointInDirection(d);
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

        private float fixedDelta;

        // private float deaccelerationSpeed;

        private float inertiaTimeout;

        // private float scrollToPointDuration;

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
            UP,
            DOWN,
            MOVING
        }
    }
}
