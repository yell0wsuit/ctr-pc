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
            sp = this.getScroll();
            mp = this.getMaxScroll();
            float num = (float)this.container.width / (float)this.width;
            float num2 = (float)this.container.height / (float)this.height;
            sc = CTRMathHelper.vect(num, num2);
        }

        public override int addChildwithID(BaseElement c, int i)
        {
            int num = this.container.addChildwithID(c, i);
            c.parentAnchor = 9;
            return num;
        }

        public override int addChild(BaseElement c)
        {
            c.parentAnchor = 9;
            return this.container.addChild(c);
        }

        public override void removeChildWithID(int i)
        {
            this.container.removeChildWithID(i);
        }

        public override void removeChild(BaseElement c)
        {
            this.container.removeChild(c);
        }

        public override BaseElement getChild(int i)
        {
            return this.container.getChild(i);
        }

        public override int childsCount()
        {
            return this.container.childsCount();
        }

        public override void draw()
        {
            float num = this.container.x;
            float num2 = this.container.y;
            this.container.x = (float)Math.Round((double)this.container.x);
            this.container.y = (float)Math.Round((double)this.container.y);
            base.preDraw();
            OpenGL.glEnable(4);
            OpenGL.setScissorRectangle(this.drawX, this.drawY, (float)this.width, (float)this.height);
            this.postDraw();
            OpenGL.glDisable(4);
            this.container.x = num;
            this.container.y = num2;
        }

        public override void postDraw()
        {
            if (!this.passTransformationsToChilds)
            {
                BaseElement.restoreTransformations(this);
            }
            this.container.preDraw();
            if (!this.container.passTransformationsToChilds)
            {
                BaseElement.restoreTransformations(this.container);
            }
            Dictionary<int, BaseElement> dictionary = this.container.getChilds();
            int i = 0;
            int count = dictionary.Count;
            while (i < count)
            {
                BaseElement baseElement = dictionary[i];
                float num = baseElement.drawX;
                float num2 = baseElement.drawY;
                if (baseElement != null && baseElement.visible && CTRMathHelper.rectInRect(num, num2, num + (float)baseElement.width, num2 + (float)baseElement.height, this.drawX, this.drawY, this.drawX + (float)this.width, this.drawY + (float)this.height))
                {
                    baseElement.draw();
                }
                else
                {
                    BaseElement.calculateTopLeft(baseElement);
                }
                i++;
            }
            if (this.container.passTransformationsToChilds)
            {
                BaseElement.restoreTransformations(this.container);
            }
            if (this.passTransformationsToChilds)
            {
                BaseElement.restoreTransformations(this);
            }
        }

        public override void update(float delta)
        {
            base.update(delta);
            delta = this.fixedDelta;
            this.targetPoint = CTRMathHelper.vectZero;
            if ((double)this.touchTimer > 0.0)
            {
                this.touchTimer -= delta;
                if ((double)this.touchTimer <= 0.0)
                {
                    this.touchTimer = 0f;
                    this.passTouches = true;
                    if (base.onTouchDownXY(this.savedTouch.x, this.savedTouch.y))
                    {
                        return;
                    }
                }
            }
            if ((double)this.touchReleaseTimer > 0.0)
            {
                this.touchReleaseTimer -= delta;
                if ((double)this.touchReleaseTimer <= 0.0)
                {
                    this.touchReleaseTimer = 0f;
                    if (base.onTouchUpXY(this.savedTouch.x, this.savedTouch.y))
                    {
                        return;
                    }
                }
            }
            if (this.touchState == ScrollableContainer.TOUCH_STATE.TOUCH_STATE_UP)
            {
                if (this.shouldBounceHorizontally)
                {
                    if ((double)this.container.x > 0.0)
                    {
                        float speed = (float)(50.0 + (double)Math.Abs(this.container.x) * 5.0);
                        this.moveToPointDeltaSpeed(CTRMathHelper.vect(0f, this.container.y), delta, speed);
                    }
                    else if (this.container.x < (float)(-(float)this.container.width + this.width) && (double)this.container.x < 0.0)
                    {
                        float speed2 = (float)(50.0 + (double)Math.Abs((float)(-(float)this.container.width + this.width) - this.container.x) * 5.0);
                        this.moveToPointDeltaSpeed(CTRMathHelper.vect((float)(-(float)this.container.width + this.width), this.container.y), delta, speed2);
                    }
                }
                if (this.shouldBounceVertically)
                {
                    if ((double)this.container.y > 0.0)
                    {
                        this.moveToPointDeltaSpeed(CTRMathHelper.vect(this.container.x, 0f), delta, (float)(50.0 + (double)Math.Abs(this.container.y) * 5.0));
                    }
                    else if (this.container.y < (float)(-(float)this.container.height + this.height) && (double)this.container.y < 0.0)
                    {
                        this.moveToPointDeltaSpeed(CTRMathHelper.vect(this.container.x, (float)(-(float)this.container.height + this.height)), delta, (float)(50.0 + (double)Math.Abs((float)(-(float)this.container.height + this.height) - this.container.y) * 5.0));
                    }
                }
            }
            if (this.movingToSpoint)
            {
                Vector vector = this.spoints[this.targetSpoint];
                this.moveToPointDeltaSpeed(vector, delta, (float)Math.Max(100.0, (double)CTRMathHelper.vectDistance(vector, CTRMathHelper.vect(this.container.x, this.container.y)) * 4.0 * (double)this.spointMoveMultiplier));
                if (this.container.x == vector.x && this.container.y == vector.y)
                {
                    this.delegateScrollableContainerProtocol?.scrollableContainerreachedScrollPoint(this, this.targetSpoint);
                    this.movingToSpoint = false;
                    this.targetSpoint = -1;
                    this.lastTargetSpoint = -1;
                    this.move = CTRMathHelper.vectZero;
                }
            }
            else if (this.canSkipScrollPoints && this.spointsNum > 0 && !CTRMathHelper.vectEqual(this.move, CTRMathHelper.vectZero) && (double)CTRMathHelper.vectLength(this.move) < 150.0 && this.targetSpoint == -1)
            {
                this.startMovingToSpointInDirection(this.move);
            }
            if (!CTRMathHelper.vectEqual(this.move, CTRMathHelper.vectZero))
            {
                CTRMathHelper.vectEqual(this.targetPoint, CTRMathHelper.vectZero);
                CTRMathHelper.vect(this.container.x, this.container.y);
                Vector v = CTRMathHelper.vectMult(CTRMathHelper.vectNeg(this.move), 2f);
                this.move = CTRMathHelper.vectAdd(this.move, CTRMathHelper.vectMult(v, delta));
                Vector off = CTRMathHelper.vectMult(this.move, delta);
                if ((double)Math.Abs(off.x) < 0.2)
                {
                    off.x = 0f;
                    this.move.x = 0f;
                }
                if ((double)Math.Abs(off.y) < 0.2)
                {
                    off.y = 0f;
                    this.move.y = 0f;
                }
                this.moveContainerBy(off);
            }
            if ((double)this.inertiaTimeoutLeft > 0.0)
            {
                this.inertiaTimeoutLeft -= delta;
            }
        }

        public override void show()
        {
            this.touchTimer = 0f;
            this.passTouches = false;
            this.touchReleaseTimer = 0f;
            this.move = CTRMathHelper.vectZero;
            if (this.resetScrollOnShow)
            {
                this.setScroll(CTRMathHelper.vectZero);
            }
        }

        public override bool onTouchDownXY(float tx, float ty)
        {
            if (!CTRMathHelper.pointInRect(tx, ty, this.drawX, this.drawY, (float)this.width, (float)this.height))
            {
                return false;
            }
            if (this.touchPassTimeout == 0f)
            {
                bool flag = base.onTouchDownXY(tx, ty);
                if (this.dontHandleTouchDownsHandledByChilds && flag)
                {
                    return true;
                }
            }
            else
            {
                this.touchTimer = this.touchPassTimeout;
                this.savedTouch = CTRMathHelper.vect(tx, ty);
                this.totalDrag = CTRMathHelper.vectZero;
                this.passTouches = false;
            }
            this.touchState = ScrollableContainer.TOUCH_STATE.TOUCH_STATE_DOWN;
            this.movingByInertion = false;
            this.movingToSpoint = false;
            this.targetSpoint = -1;
            this.dragStart = CTRMathHelper.vect(tx, ty);
            return true;
        }

        public override bool onTouchMoveXY(float tx, float ty)
        {
            if (this.touchPassTimeout == 0f || this.passTouches)
            {
                bool flag = base.onTouchMoveXY(tx, ty);
                if (this.dontHandleTouchMovesHandledByChilds && flag)
                {
                    return true;
                }
            }
            Vector vector = CTRMathHelper.vect(tx, ty);
            if (CTRMathHelper.vectEqual(this.dragStart, vector))
            {
                return false;
            }
            if (CTRMathHelper.vectEqual(this.dragStart, ScrollableContainer.impossibleTouch) && !CTRMathHelper.pointInRect(tx, ty, this.drawX, this.drawY, (float)this.width, (float)this.height))
            {
                return false;
            }
            this.touchState = ScrollableContainer.TOUCH_STATE.TOUCH_STATE_MOVING;
            if (!CTRMathHelper.vectEqual(this.dragStart, ScrollableContainer.impossibleTouch))
            {
                Vector vector2 = CTRMathHelper.vectSub(vector, this.dragStart);
                this.dragStart = vector;
                vector2.x = CTRMathHelper.FIT_TO_BOUNDARIES(vector2.x, 0f - this.maxTouchMoveLength, this.maxTouchMoveLength);
                vector2.y = CTRMathHelper.FIT_TO_BOUNDARIES(vector2.y, 0f - this.maxTouchMoveLength, this.maxTouchMoveLength);
                this.totalDrag = CTRMathHelper.vectAdd(this.totalDrag, vector2);
                if (((double)this.touchTimer > 0.0 || this.untouchChildsOnMove) && CTRMathHelper.vectLength(this.totalDrag) > this.touchMoveIgnoreLength)
                {
                    this.touchTimer = 0f;
                    this.passTouches = false;
                    base.onTouchUpXY(-1f, -1f);
                }
                if (this.container.width <= this.width)
                {
                    vector2.x = 0f;
                }
                if (this.container.height <= this.height)
                {
                    vector2.y = 0f;
                }
                if (this.shouldBounceHorizontally && ((double)this.container.x > 0.0 || this.container.x < (float)(-(float)this.container.width + this.width)))
                {
                    vector2.x /= 2f;
                }
                if (this.shouldBounceVertically && ((double)this.container.y > 0.0 || this.container.y < (float)(-(float)this.container.height + this.height)))
                {
                    vector2.y /= 2f;
                }
                this.staticMove = this.moveContainerBy(vector2);
                this.move = CTRMathHelper.vectZero;
                this.inertiaTimeoutLeft = this.inertiaTimeout;
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
            if (this.touchPassTimeout == 0f || this.passTouches)
            {
                bool flag = base.onTouchUpXY(tx, ty);
                if (this.dontHandleTouchUpsHandledByChilds && flag)
                {
                    return true;
                }
            }
            if ((double)this.touchTimer > 0.0)
            {
                bool flag2 = base.onTouchDownXY(this.savedTouch.x, this.savedTouch.y);
                this.touchReleaseTimer = 0.2f;
                this.touchTimer = 0f;
                if (this.dontHandleTouchDownsHandledByChilds && flag2)
                {
                    return true;
                }
            }
            if (this.touchState == ScrollableContainer.TOUCH_STATE.TOUCH_STATE_UP)
            {
                return false;
            }
            this.touchState = ScrollableContainer.TOUCH_STATE.TOUCH_STATE_UP;
            if ((double)this.inertiaTimeoutLeft > 0.0)
            {
                float num = this.inertiaTimeoutLeft / this.inertiaTimeout;
                this.move = CTRMathHelper.vectMult(this.staticMove, (float)((double)num * 50.0));
                this.movingByInertion = true;
            }
            if (this.spointsNum > 0)
            {
                if (!this.canSkipScrollPoints)
                {
                    if (this.minAutoScrollToSpointLength != -1f && CTRMathHelper.vectLength(this.move) > this.minAutoScrollToSpointLength)
                    {
                        this.startMovingToSpointInDirection(this.move);
                    }
                    else
                    {
                        this.startMovingToSpointInDirection(CTRMathHelper.vectZero);
                    }
                }
                else if (CTRMathHelper.vectEqual(this.move, CTRMathHelper.vectZero))
                {
                    this.startMovingToSpointInDirection(CTRMathHelper.vectZero);
                }
            }
            this.dragStart = ScrollableContainer.impossibleTouch;
            return true;
        }

        public override void dealloc()
        {
            this.spoints = null;
            base.dealloc();
        }

        public virtual ScrollableContainer initWithWidthHeightContainer(float w, float h, BaseElement c)
        {
            if (this.init() != null)
            {
                float num = (float)Application.sharedAppSettings().getInt(5);
                this.fixedDelta = (float)(1.0 / (double)num);
                this.spoints = null;
                this.spointsNum = -1;
                this.spointsCapacity = -1;
                this.targetSpoint = -1;
                this.lastTargetSpoint = -1;
                this.deaccelerationSpeed = 3f;
                this.inertiaTimeout = 0.1f;
                this.scrollToPointDuration = 0.35f;
                this.canSkipScrollPoints = false;
                this.shouldBounceHorizontally = false;
                this.shouldBounceVertically = false;
                this.touchMoveIgnoreLength = 0f;
                this.maxTouchMoveLength = 40f;
                this.touchPassTimeout = 0.5f;
                this.minAutoScrollToSpointLength = -1f;
                this.resetScrollOnShow = true;
                this.untouchChildsOnMove = false;
                this.dontHandleTouchDownsHandledByChilds = false;
                this.dontHandleTouchMovesHandledByChilds = false;
                this.dontHandleTouchUpsHandledByChilds = false;
                this.touchTimer = 0f;
                this.passTouches = false;
                this.touchReleaseTimer = 0f;
                this.move = CTRMathHelper.vectZero;
                this.container = c;
                this.width = (int)w;
                this.height = (int)h;
                this.container.parentAnchor = 9;
                this.container.parent = this;
                this.childs[0] = this.container;
                this.dragStart = ScrollableContainer.impossibleTouch;
                this.touchState = ScrollableContainer.TOUCH_STATE.TOUCH_STATE_UP;
            }
            return this;
        }

        public virtual ScrollableContainer initWithWidthHeightContainerWidthHeight(float w, float h, float cw, float ch)
        {
            this.container = (BaseElement)new BaseElement().init();
            this.container.width = (int)cw;
            this.container.height = (int)ch;
            this.initWithWidthHeightContainer(w, h, this.container);
            return this;
        }

        public virtual void turnScrollPointsOnWithCapacity(int n)
        {
            this.spointsCapacity = n;
            this.spoints = new Vector[this.spointsCapacity];
            this.spointsNum = 0;
        }

        public virtual int addScrollPointAtXY(double sx, double sy)
        {
            return this.addScrollPointAtXY((float)sx, (float)sy);
        }

        public virtual int addScrollPointAtXY(float sx, float sy)
        {
            this.addScrollPointAtXYwithID(sx, sy, this.spointsNum);
            return this.spointsNum - 1;
        }

        public virtual void addScrollPointAtXYwithID(float sx, float sy, int i)
        {
            this.spoints[i] = CTRMathHelper.vect(0f - sx, 0f - sy);
            if (i > this.spointsNum - 1)
            {
                this.spointsNum = i + 1;
            }
        }

        public virtual int getTotalScrollPoints()
        {
            return this.spointsNum;
        }

        public virtual Vector getScrollPoint(int i)
        {
            return this.spoints[i];
        }

        public virtual Vector getScroll()
        {
            return CTRMathHelper.vect(0f - this.container.x, 0f - this.container.y);
        }

        public virtual Vector getMaxScroll()
        {
            return CTRMathHelper.vect((float)(this.container.width - this.width), (float)(this.container.height - this.height));
        }

        public virtual void setScroll(Vector s)
        {
            this.move = CTRMathHelper.vectZero;
            this.container.x = 0f - s.x;
            this.container.y = 0f - s.y;
            this.movingToSpoint = false;
            this.targetSpoint = -1;
            this.lastTargetSpoint = -1;
        }

        public virtual void placeToScrollPoint(int sp)
        {
            this.move = CTRMathHelper.vectZero;
            this.container.x = this.spoints[sp].x;
            this.container.y = this.spoints[sp].y;
            this.movingToSpoint = false;
            this.targetSpoint = -1;
            this.lastTargetSpoint = sp;
            this.delegateScrollableContainerProtocol?.scrollableContainerreachedScrollPoint(this, sp);
        }

        public virtual void moveToScrollPointmoveMultiplier(int sp, double m)
        {
            this.moveToScrollPointmoveMultiplier(sp, (float)m);
        }

        public virtual void moveToScrollPointmoveMultiplier(int sp, float m)
        {
            this.movingToSpoint = true;
            this.movingByInertion = false;
            this.spointMoveMultiplier = m;
            this.targetSpoint = sp;
            this.lastTargetSpoint = this.targetSpoint;
        }

        public virtual void calculateNearsetScrollPointInDirection(Vector d)
        {
            this.spointMoveDirection = d;
            int num = -1;
            float num2 = 9999999f;
            float num3 = CTRMathHelper.angleTo0_360(CTRMathHelper.RADIANS_TO_DEGREES(CTRMathHelper.vectAngleNormalized(d)));
            Vector v = CTRMathHelper.vect(this.container.x, this.container.y);
            for (int i = 0; i < this.spointsNum; i++)
            {
                if ((double)this.spoints[i].x <= 0.0 && (this.spoints[i].x >= (float)(-(float)this.container.width + this.width) || (double)this.spoints[i].x >= 0.0) && (double)this.spoints[i].y <= 0.0 && (this.spoints[i].y >= (float)(-(float)this.container.height + this.height) || (double)this.spoints[i].y >= 0.0))
                {
                    float num4 = CTRMathHelper.vectDistance(this.spoints[i], v);
                    if ((CTRMathHelper.vectEqual(d, CTRMathHelper.vectZero) || Math.Abs(CTRMathHelper.angleTo0_360(CTRMathHelper.RADIANS_TO_DEGREES(CTRMathHelper.vectAngleNormalized(CTRMathHelper.vectSub(this.spoints[i], v)))) - num3) <= 90f) && num4 < num2)
                    {
                        num = i;
                        num2 = num4;
                    }
                }
            }
            if (num == -1 && !CTRMathHelper.vectEqual(d, CTRMathHelper.vectZero))
            {
                this.calculateNearsetScrollPointInDirection(CTRMathHelper.vectZero);
                return;
            }
            this.targetSpoint = num;
            if (!this.canSkipScrollPoints && this.targetSpoint != this.lastTargetSpoint)
            {
                this.movingByInertion = false;
            }
            if (this.lastTargetSpoint != this.targetSpoint && this.targetSpoint != -1 && this.delegateScrollableContainerProtocol != null)
            {
                this.delegateScrollableContainerProtocol.scrollableContainerchangedTargetScrollPoint(this, this.targetSpoint);
            }
            float num6 = CTRMathHelper.angleTo0_360(CTRMathHelper.RADIANS_TO_DEGREES(CTRMathHelper.vectAngleNormalized(this.move)));
            float num5 = CTRMathHelper.angleTo0_360(CTRMathHelper.RADIANS_TO_DEGREES(CTRMathHelper.vectAngleNormalized(CTRMathHelper.vectSub(this.spoints[this.targetSpoint], v))));
            if (Math.Abs(CTRMathHelper.angleTo0_360(num6 - num5)) < 90f)
            {
                this.spointMoveMultiplier = (float)Math.Max(1.0, (double)CTRMathHelper.vectLength(this.move) / 500.0);
            }
            else
            {
                this.spointMoveMultiplier = 0.5f;
            }
            this.lastTargetSpoint = this.targetSpoint;
        }

        public virtual Vector moveContainerBy(Vector off)
        {
            float val = this.container.x + off.x;
            float val2 = this.container.y + off.y;
            if (!this.shouldBounceHorizontally)
            {
                val = (float)Math.Min((double)Math.Max((float)(-(float)this.container.width + this.width), val), 0.0);
            }
            if (!this.shouldBounceVertically)
            {
                val2 = (float)Math.Min((double)Math.Max((float)(-(float)this.container.height + this.height), val2), 0.0);
            }
            Vector vector = CTRMathHelper.vectSub(CTRMathHelper.vect(val, val2), CTRMathHelper.vect(this.container.x, this.container.y));
            this.container.x = val;
            this.container.y = val2;
            return vector;
        }

        public virtual void moveToPointDeltaSpeed(Vector tsp, float delta, float speed)
        {
            Vector v = CTRMathHelper.vectSub(tsp, CTRMathHelper.vect(this.container.x, this.container.y));
            v = CTRMathHelper.vectNormalize(v);
            v = CTRMathHelper.vectMult(v, speed);
            Mover.moveVariableToTarget(ref this.container.x, tsp.x, Math.Abs(v.x), delta);
            Mover.moveVariableToTarget(ref this.container.y, tsp.y, Math.Abs(v.y), delta);
            this.targetPoint = tsp;
            this.move = CTRMathHelper.vectZero;
        }

        public virtual void startMovingToSpointInDirection(Vector d)
        {
            this.movingToSpoint = true;
            this.targetSpoint = this.lastTargetSpoint = -1;
            this.calculateNearsetScrollPointInDirection(d);
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

        private ScrollableContainer.TOUCH_STATE touchState;

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
