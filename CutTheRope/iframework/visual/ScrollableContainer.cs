using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.windows;
using System;
using System.Collections.Generic;

namespace CutTheRope.iframework.visual
{
    // Token: 0x02000046 RID: 70
    internal class ScrollableContainer : BaseElement
    {
        // Token: 0x0600023B RID: 571 RVA: 0x0000BF5C File Offset: 0x0000A15C
        public void provideScrollPosMaxScrollPosScrollCoeff(ref Vector sp, ref Vector mp, ref Vector sc)
        {
            sp = this.getScroll();
            mp = this.getMaxScroll();
            float num = (float)this.container.width / (float)this.width;
            float num2 = (float)this.container.height / (float)this.height;
            sc = MathHelper.vect(num, num2);
        }

        // Token: 0x0600023C RID: 572 RVA: 0x0000BFB8 File Offset: 0x0000A1B8
        public override int addChildwithID(BaseElement c, int i)
        {
            int num = this.container.addChildwithID(c, i);
            c.parentAnchor = 9;
            return num;
        }

        // Token: 0x0600023D RID: 573 RVA: 0x0000BFCF File Offset: 0x0000A1CF
        public override int addChild(BaseElement c)
        {
            c.parentAnchor = 9;
            return this.container.addChild(c);
        }

        // Token: 0x0600023E RID: 574 RVA: 0x0000BFE5 File Offset: 0x0000A1E5
        public override void removeChildWithID(int i)
        {
            this.container.removeChildWithID(i);
        }

        // Token: 0x0600023F RID: 575 RVA: 0x0000BFF3 File Offset: 0x0000A1F3
        public override void removeChild(BaseElement c)
        {
            this.container.removeChild(c);
        }

        // Token: 0x06000240 RID: 576 RVA: 0x0000C001 File Offset: 0x0000A201
        public override BaseElement getChild(int i)
        {
            return this.container.getChild(i);
        }

        // Token: 0x06000241 RID: 577 RVA: 0x0000C00F File Offset: 0x0000A20F
        public override int childsCount()
        {
            return this.container.childsCount();
        }

        // Token: 0x06000242 RID: 578 RVA: 0x0000C01C File Offset: 0x0000A21C
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

        // Token: 0x06000243 RID: 579 RVA: 0x0000C0CC File Offset: 0x0000A2CC
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
                if (baseElement != null && baseElement.visible && MathHelper.rectInRect(num, num2, num + (float)baseElement.width, num2 + (float)baseElement.height, this.drawX, this.drawY, this.drawX + (float)this.width, this.drawY + (float)this.height))
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

        // Token: 0x06000244 RID: 580 RVA: 0x0000C1C8 File Offset: 0x0000A3C8
        public override void update(float delta)
        {
            base.update(delta);
            delta = this.fixedDelta;
            this.targetPoint = MathHelper.vectZero;
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
                        this.moveToPointDeltaSpeed(MathHelper.vect(0f, this.container.y), delta, speed);
                    }
                    else if (this.container.x < (float)(-(float)this.container.width + this.width) && (double)this.container.x < 0.0)
                    {
                        float speed2 = (float)(50.0 + (double)Math.Abs((float)(-(float)this.container.width + this.width) - this.container.x) * 5.0);
                        this.moveToPointDeltaSpeed(MathHelper.vect((float)(-(float)this.container.width + this.width), this.container.y), delta, speed2);
                    }
                }
                if (this.shouldBounceVertically)
                {
                    if ((double)this.container.y > 0.0)
                    {
                        this.moveToPointDeltaSpeed(MathHelper.vect(this.container.x, 0f), delta, (float)(50.0 + (double)Math.Abs(this.container.y) * 5.0));
                    }
                    else if (this.container.y < (float)(-(float)this.container.height + this.height) && (double)this.container.y < 0.0)
                    {
                        this.moveToPointDeltaSpeed(MathHelper.vect(this.container.x, (float)(-(float)this.container.height + this.height)), delta, (float)(50.0 + (double)Math.Abs((float)(-(float)this.container.height + this.height) - this.container.y) * 5.0));
                    }
                }
            }
            if (this.movingToSpoint)
            {
                Vector vector = this.spoints[this.targetSpoint];
                this.moveToPointDeltaSpeed(vector, delta, (float)Math.Max(100.0, (double)MathHelper.vectDistance(vector, MathHelper.vect(this.container.x, this.container.y)) * 4.0 * (double)this.spointMoveMultiplier));
                if (this.container.x == vector.x && this.container.y == vector.y)
                {
                    if (this.delegateScrollableContainerProtocol != null)
                    {
                        this.delegateScrollableContainerProtocol.scrollableContainerreachedScrollPoint(this, this.targetSpoint);
                    }
                    this.movingToSpoint = false;
                    this.targetSpoint = -1;
                    this.lastTargetSpoint = -1;
                    this.move = MathHelper.vectZero;
                }
            }
            else if (this.canSkipScrollPoints && this.spointsNum > 0 && !MathHelper.vectEqual(this.move, MathHelper.vectZero) && (double)MathHelper.vectLength(this.move) < 150.0 && this.targetSpoint == -1)
            {
                this.startMovingToSpointInDirection(this.move);
            }
            if (!MathHelper.vectEqual(this.move, MathHelper.vectZero))
            {
                MathHelper.vectEqual(this.targetPoint, MathHelper.vectZero);
                MathHelper.vect(this.container.x, this.container.y);
                Vector v = MathHelper.vectMult(MathHelper.vectNeg(this.move), 2f);
                this.move = MathHelper.vectAdd(this.move, MathHelper.vectMult(v, delta));
                Vector off = MathHelper.vectMult(this.move, delta);
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

        // Token: 0x06000245 RID: 581 RVA: 0x0000C6F9 File Offset: 0x0000A8F9
        public override void show()
        {
            this.touchTimer = 0f;
            this.passTouches = false;
            this.touchReleaseTimer = 0f;
            this.move = MathHelper.vectZero;
            if (this.resetScrollOnShow)
            {
                this.setScroll(MathHelper.vectZero);
            }
        }

        // Token: 0x06000246 RID: 582 RVA: 0x0000C738 File Offset: 0x0000A938
        public override bool onTouchDownXY(float tx, float ty)
        {
            if (!MathHelper.pointInRect(tx, ty, this.drawX, this.drawY, (float)this.width, (float)this.height))
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
                this.savedTouch = MathHelper.vect(tx, ty);
                this.totalDrag = MathHelper.vectZero;
                this.passTouches = false;
            }
            this.touchState = ScrollableContainer.TOUCH_STATE.TOUCH_STATE_DOWN;
            this.movingByInertion = false;
            this.movingToSpoint = false;
            this.targetSpoint = -1;
            this.dragStart = MathHelper.vect(tx, ty);
            return true;
        }

        // Token: 0x06000247 RID: 583 RVA: 0x0000C7E4 File Offset: 0x0000A9E4
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
            Vector vector = MathHelper.vect(tx, ty);
            if (MathHelper.vectEqual(this.dragStart, vector))
            {
                return false;
            }
            if (MathHelper.vectEqual(this.dragStart, ScrollableContainer.impossibleTouch) && !MathHelper.pointInRect(tx, ty, this.drawX, this.drawY, (float)this.width, (float)this.height))
            {
                return false;
            }
            this.touchState = ScrollableContainer.TOUCH_STATE.TOUCH_STATE_MOVING;
            if (!MathHelper.vectEqual(this.dragStart, ScrollableContainer.impossibleTouch))
            {
                Vector vector2 = MathHelper.vectSub(vector, this.dragStart);
                this.dragStart = vector;
                vector2.x = MathHelper.FIT_TO_BOUNDARIES(vector2.x, 0f - this.maxTouchMoveLength, this.maxTouchMoveLength);
                vector2.y = MathHelper.FIT_TO_BOUNDARIES(vector2.y, 0f - this.maxTouchMoveLength, this.maxTouchMoveLength);
                this.totalDrag = MathHelper.vectAdd(this.totalDrag, vector2);
                if (((double)this.touchTimer > 0.0 || this.untouchChildsOnMove) && MathHelper.vectLength(this.totalDrag) > this.touchMoveIgnoreLength)
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
                this.move = MathHelper.vectZero;
                this.inertiaTimeoutLeft = this.inertiaTimeout;
                return true;
            }
            return false;
        }

        // Token: 0x06000248 RID: 584 RVA: 0x0000CA4C File Offset: 0x0000AC4C
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
                this.move = MathHelper.vectMult(this.staticMove, (float)((double)num * 50.0));
                this.movingByInertion = true;
            }
            if (this.spointsNum > 0)
            {
                if (!this.canSkipScrollPoints)
                {
                    if (this.minAutoScrollToSpointLength != -1f && MathHelper.vectLength(this.move) > this.minAutoScrollToSpointLength)
                    {
                        this.startMovingToSpointInDirection(this.move);
                    }
                    else
                    {
                        this.startMovingToSpointInDirection(MathHelper.vectZero);
                    }
                }
                else if (MathHelper.vectEqual(this.move, MathHelper.vectZero))
                {
                    this.startMovingToSpointInDirection(MathHelper.vectZero);
                }
            }
            this.dragStart = ScrollableContainer.impossibleTouch;
            return true;
        }

        // Token: 0x06000249 RID: 585 RVA: 0x0000CBB1 File Offset: 0x0000ADB1
        public override void dealloc()
        {
            this.spoints = null;
            base.dealloc();
        }

        // Token: 0x0600024A RID: 586 RVA: 0x0000CBC0 File Offset: 0x0000ADC0
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
                this.move = MathHelper.vectZero;
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

        // Token: 0x0600024B RID: 587 RVA: 0x0000CD20 File Offset: 0x0000AF20
        public virtual ScrollableContainer initWithWidthHeightContainerWidthHeight(float w, float h, float cw, float ch)
        {
            this.container = (BaseElement)new BaseElement().init();
            this.container.width = (int)cw;
            this.container.height = (int)ch;
            this.initWithWidthHeightContainer(w, h, this.container);
            return this;
        }

        // Token: 0x0600024C RID: 588 RVA: 0x0000CD6D File Offset: 0x0000AF6D
        public virtual void turnScrollPointsOnWithCapacity(int n)
        {
            this.spointsCapacity = n;
            this.spoints = new Vector[this.spointsCapacity];
            this.spointsNum = 0;
        }

        // Token: 0x0600024D RID: 589 RVA: 0x0000CD8E File Offset: 0x0000AF8E
        public virtual int addScrollPointAtXY(double sx, double sy)
        {
            return this.addScrollPointAtXY((float)sx, (float)sy);
        }

        // Token: 0x0600024E RID: 590 RVA: 0x0000CD9A File Offset: 0x0000AF9A
        public virtual int addScrollPointAtXY(float sx, float sy)
        {
            this.addScrollPointAtXYwithID(sx, sy, this.spointsNum);
            return this.spointsNum - 1;
        }

        // Token: 0x0600024F RID: 591 RVA: 0x0000CDB2 File Offset: 0x0000AFB2
        public virtual void addScrollPointAtXYwithID(float sx, float sy, int i)
        {
            this.spoints[i] = MathHelper.vect(0f - sx, 0f - sy);
            if (i > this.spointsNum - 1)
            {
                this.spointsNum = i + 1;
            }
        }

        // Token: 0x06000250 RID: 592 RVA: 0x0000CDE7 File Offset: 0x0000AFE7
        public virtual int getTotalScrollPoints()
        {
            return this.spointsNum;
        }

        // Token: 0x06000251 RID: 593 RVA: 0x0000CDEF File Offset: 0x0000AFEF
        public virtual Vector getScrollPoint(int i)
        {
            return this.spoints[i];
        }

        // Token: 0x06000252 RID: 594 RVA: 0x0000CDFD File Offset: 0x0000AFFD
        public virtual Vector getScroll()
        {
            return MathHelper.vect(0f - this.container.x, 0f - this.container.y);
        }

        // Token: 0x06000253 RID: 595 RVA: 0x0000CE26 File Offset: 0x0000B026
        public virtual Vector getMaxScroll()
        {
            return MathHelper.vect((float)(this.container.width - this.width), (float)(this.container.height - this.height));
        }

        // Token: 0x06000254 RID: 596 RVA: 0x0000CE54 File Offset: 0x0000B054
        public virtual void setScroll(Vector s)
        {
            this.move = MathHelper.vectZero;
            this.container.x = 0f - s.x;
            this.container.y = 0f - s.y;
            this.movingToSpoint = false;
            this.targetSpoint = -1;
            this.lastTargetSpoint = -1;
        }

        // Token: 0x06000255 RID: 597 RVA: 0x0000CEB0 File Offset: 0x0000B0B0
        public virtual void placeToScrollPoint(int sp)
        {
            this.move = MathHelper.vectZero;
            this.container.x = this.spoints[sp].x;
            this.container.y = this.spoints[sp].y;
            this.movingToSpoint = false;
            this.targetSpoint = -1;
            this.lastTargetSpoint = sp;
            if (this.delegateScrollableContainerProtocol != null)
            {
                this.delegateScrollableContainerProtocol.scrollableContainerreachedScrollPoint(this, sp);
            }
        }

        // Token: 0x06000256 RID: 598 RVA: 0x0000CF2A File Offset: 0x0000B12A
        public virtual void moveToScrollPointmoveMultiplier(int sp, double m)
        {
            this.moveToScrollPointmoveMultiplier(sp, (float)m);
        }

        // Token: 0x06000257 RID: 599 RVA: 0x0000CF35 File Offset: 0x0000B135
        public virtual void moveToScrollPointmoveMultiplier(int sp, float m)
        {
            this.movingToSpoint = true;
            this.movingByInertion = false;
            this.spointMoveMultiplier = m;
            this.targetSpoint = sp;
            this.lastTargetSpoint = this.targetSpoint;
        }

        // Token: 0x06000258 RID: 600 RVA: 0x0000CF60 File Offset: 0x0000B160
        public virtual void calculateNearsetScrollPointInDirection(Vector d)
        {
            this.spointMoveDirection = d;
            int num = -1;
            float num2 = 9999999f;
            float num3 = MathHelper.angleTo0_360(MathHelper.RADIANS_TO_DEGREES(MathHelper.vectAngleNormalized(d)));
            Vector v = MathHelper.vect(this.container.x, this.container.y);
            for (int i = 0; i < this.spointsNum; i++)
            {
                if ((double)this.spoints[i].x <= 0.0 && (this.spoints[i].x >= (float)(-(float)this.container.width + this.width) || (double)this.spoints[i].x >= 0.0) && (double)this.spoints[i].y <= 0.0 && (this.spoints[i].y >= (float)(-(float)this.container.height + this.height) || (double)this.spoints[i].y >= 0.0))
                {
                    float num4 = MathHelper.vectDistance(this.spoints[i], v);
                    if ((MathHelper.vectEqual(d, MathHelper.vectZero) || Math.Abs(MathHelper.angleTo0_360(MathHelper.RADIANS_TO_DEGREES(MathHelper.vectAngleNormalized(MathHelper.vectSub(this.spoints[i], v)))) - num3) <= 90f) && num4 < num2)
                    {
                        num = i;
                        num2 = num4;
                    }
                }
            }
            if (num == -1 && !MathHelper.vectEqual(d, MathHelper.vectZero))
            {
                this.calculateNearsetScrollPointInDirection(MathHelper.vectZero);
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
            float num6 = MathHelper.angleTo0_360(MathHelper.RADIANS_TO_DEGREES(MathHelper.vectAngleNormalized(this.move)));
            float num5 = MathHelper.angleTo0_360(MathHelper.RADIANS_TO_DEGREES(MathHelper.vectAngleNormalized(MathHelper.vectSub(this.spoints[this.targetSpoint], v))));
            if (Math.Abs(MathHelper.angleTo0_360(num6 - num5)) < 90f)
            {
                this.spointMoveMultiplier = (float)Math.Max(1.0, (double)MathHelper.vectLength(this.move) / 500.0);
            }
            else
            {
                this.spointMoveMultiplier = 0.5f;
            }
            this.lastTargetSpoint = this.targetSpoint;
        }

        // Token: 0x06000259 RID: 601 RVA: 0x0000D1FC File Offset: 0x0000B3FC
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
            Vector vector = MathHelper.vectSub(MathHelper.vect(val, val2), MathHelper.vect(this.container.x, this.container.y));
            this.container.x = val;
            this.container.y = val2;
            return vector;
        }

        // Token: 0x0600025A RID: 602 RVA: 0x0000D2D4 File Offset: 0x0000B4D4
        public virtual void moveToPointDeltaSpeed(Vector tsp, float delta, float speed)
        {
            Vector v = MathHelper.vectSub(tsp, MathHelper.vect(this.container.x, this.container.y));
            v = MathHelper.vectNormalize(v);
            v = MathHelper.vectMult(v, speed);
            Mover.moveVariableToTarget(ref this.container.x, tsp.x, Math.Abs(v.x), delta);
            Mover.moveVariableToTarget(ref this.container.y, tsp.y, Math.Abs(v.y), delta);
            this.targetPoint = tsp;
            this.move = MathHelper.vectZero;
        }

        // Token: 0x0600025B RID: 603 RVA: 0x0000D36C File Offset: 0x0000B56C
        public virtual void startMovingToSpointInDirection(Vector d)
        {
            this.movingToSpoint = true;
            this.targetSpoint = (this.lastTargetSpoint = -1);
            this.calculateNearsetScrollPointInDirection(d);
        }

        // Token: 0x04000193 RID: 403
        private const double DEFAULT_BOUNCE_MOVEMENT_DIVIDE = 2.0;

        // Token: 0x04000194 RID: 404
        private const double DEFAULT_BOUNCE_DURATION = 0.1;

        // Token: 0x04000195 RID: 405
        private const double DEFAULT_DEACCELERATION = 3.0;

        // Token: 0x04000196 RID: 406
        private const double DEFAULT_INERTIAL_TIMEOUT = 0.1;

        // Token: 0x04000197 RID: 407
        private const double DEFAULT_SCROLL_TO_POINT_DURATION = 0.35;

        // Token: 0x04000198 RID: 408
        private const double MIN_SCROLL_POINTS_MOVE = 50.0;

        // Token: 0x04000199 RID: 409
        private const double CALC_NEAREST_DEFAULT_TIMEOUT = 0.02;

        // Token: 0x0400019A RID: 410
        private const double DEFAULT_MAX_TOUCH_MOVE_LENGTH = 40.0;

        // Token: 0x0400019B RID: 411
        private const double DEFAULT_TOUCH_PASS_TIMEOUT = 0.5;

        // Token: 0x0400019C RID: 412
        private const double AUTO_RELEASE_TOUCH_TIMEOUT = 0.2;

        // Token: 0x0400019D RID: 413
        private const double MOVE_APPROXIMATION = 0.2;

        // Token: 0x0400019E RID: 414
        public ScrollableContainerProtocol delegateScrollableContainerProtocol;

        // Token: 0x0400019F RID: 415
        private static readonly Vector impossibleTouch = new(-1000f, -1000f);

        // Token: 0x040001A0 RID: 416
        private BaseElement container;

        // Token: 0x040001A1 RID: 417
        private Vector dragStart;

        // Token: 0x040001A2 RID: 418
        private Vector staticMove;

        // Token: 0x040001A3 RID: 419
        private Vector move;

        // Token: 0x040001A4 RID: 420
        private bool movingByInertion;

        // Token: 0x040001A5 RID: 421
        private float inertiaTimeoutLeft;

        // Token: 0x040001A6 RID: 422
        private bool movingToSpoint;

        // Token: 0x040001A7 RID: 423
        private int targetSpoint;

        // Token: 0x040001A8 RID: 424
        private int lastTargetSpoint;

        // Token: 0x040001A9 RID: 425
        private float spointMoveMultiplier;

        // Token: 0x040001AA RID: 426
        private Vector[] spoints;

        // Token: 0x040001AB RID: 427
        private int spointsNum;

        // Token: 0x040001AC RID: 428
        private int spointsCapacity;

        // Token: 0x040001AD RID: 429
        private Vector spointMoveDirection;

        // Token: 0x040001AE RID: 430
        private Vector targetPoint;

        // Token: 0x040001AF RID: 431
        private ScrollableContainer.TOUCH_STATE touchState;

        // Token: 0x040001B0 RID: 432
        public float touchTimer;

        // Token: 0x040001B1 RID: 433
        private float touchReleaseTimer;

        // Token: 0x040001B2 RID: 434
        private Vector savedTouch;

        // Token: 0x040001B3 RID: 435
        private Vector totalDrag;

        // Token: 0x040001B4 RID: 436
        public bool passTouches;

        // Token: 0x040001B5 RID: 437
        private float fixedDelta;

        // Token: 0x040001B6 RID: 438
        private float deaccelerationSpeed;

        // Token: 0x040001B7 RID: 439
        private float inertiaTimeout;

        // Token: 0x040001B8 RID: 440
        private float scrollToPointDuration;

        // Token: 0x040001B9 RID: 441
        private bool canSkipScrollPoints;

        // Token: 0x040001BA RID: 442
        public bool shouldBounceHorizontally;

        // Token: 0x040001BB RID: 443
        private bool shouldBounceVertically;

        // Token: 0x040001BC RID: 444
        public float touchMoveIgnoreLength;

        // Token: 0x040001BD RID: 445
        private float maxTouchMoveLength;

        // Token: 0x040001BE RID: 446
        private float touchPassTimeout;

        // Token: 0x040001BF RID: 447
        public bool resetScrollOnShow;

        // Token: 0x040001C0 RID: 448
        public bool dontHandleTouchDownsHandledByChilds;

        // Token: 0x040001C1 RID: 449
        public bool dontHandleTouchMovesHandledByChilds;

        // Token: 0x040001C2 RID: 450
        public bool dontHandleTouchUpsHandledByChilds;

        // Token: 0x040001C3 RID: 451
        private bool untouchChildsOnMove;

        // Token: 0x040001C4 RID: 452
        public float minAutoScrollToSpointLength;

        // Token: 0x020000AF RID: 175
        private enum TOUCH_STATE
        {
            // Token: 0x0400089C RID: 2204
            TOUCH_STATE_UP,
            // Token: 0x0400089D RID: 2205
            TOUCH_STATE_DOWN,
            // Token: 0x0400089E RID: 2206
            TOUCH_STATE_MOVING
        }
    }
}
