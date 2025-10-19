using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.ctr_commons
{
	// Token: 0x0200009C RID: 156
	internal class HLiftScrollbar : Image
	{
		// Token: 0x0600062D RID: 1581 RVA: 0x0003300F File Offset: 0x0003120F
		public static HLiftScrollbar createWithResIDBackQuadLiftQuadLiftQuadPressed(int resID, int bq, int lq, int lqp)
		{
			return new HLiftScrollbar().initWithResIDBackQuadLiftQuadLiftQuadPressed(resID, bq, lq, lqp);
		}

		// Token: 0x0600062E RID: 1582 RVA: 0x00033020 File Offset: 0x00031220
		public virtual HLiftScrollbar initWithResIDBackQuadLiftQuadLiftQuadPressed(int resID, int bq, int lq, int lqp)
		{
			if (base.initWithTexture(Application.getTexture(resID)) != null)
			{
				this.setDrawQuad(bq);
				Image up = Image.Image_createWithResIDQuad(resID, lq);
				Image image = Image.Image_createWithResIDQuad(resID, lqp);
				Vector relativeQuadOffset = Image.getRelativeQuadOffset(resID, lq, lqp);
				image.x += relativeQuadOffset.x;
				image.y += relativeQuadOffset.y;
				this.lift = (Lift)new Lift().initWithUpElementDownElementandID(up, image, 0);
				this.lift.parentAnchor = 17;
				this.lift.anchor = 18;
				this.lift.minX = 1f;
				this.lift.maxX = (float)this.width - this.lift.minX;
				this.lift.liftDelegate = new Lift.PercentXY(this.percentXY);
				int num = 45;
				this.lift.setTouchIncreaseLeftRightTopBottom((float)num, (float)num, -5f, 10f);
				this.addChild(this.lift);
				this.spointsNum = 0;
				this.spoints = null;
				this.activeSpoint = 0;
			}
			return this;
		}

		// Token: 0x0600062F RID: 1583 RVA: 0x0003313C File Offset: 0x0003133C
		public virtual Vector getScrollPoint(int i)
		{
			return this.spoints[i];
		}

		// Token: 0x06000630 RID: 1584 RVA: 0x0003314A File Offset: 0x0003134A
		public virtual int getTotalScrollPoints()
		{
			return this.spointsNum;
		}

		// Token: 0x06000631 RID: 1585 RVA: 0x00033154 File Offset: 0x00031354
		public virtual void updateActiveSpoint()
		{
			int i = 0;
			while (i < this.spointsNum)
			{
				if (this.lift.x <= this.spointsLimits[i].x)
				{
					this.activeSpoint = this.limitPoints[i];
					if (this.delegateLiftScrollbarDelegate != null)
					{
						this.delegateLiftScrollbarDelegate.changedActiveSpointFromTo(0, this.activeSpoint);
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

		// Token: 0x06000632 RID: 1586 RVA: 0x000331BC File Offset: 0x000313BC
		public override void update(float delta)
		{
			base.update(delta);
			this.updateLift();
			for (int i = 0; i < this.spointsNum; i++)
			{
				if (this.lift.x <= this.spointsLimits[i].x)
				{
					int num = this.limitPoints[i];
					if (this.activeSpoint != num)
					{
						if (this.delegateLiftScrollbarDelegate != null)
						{
							this.delegateLiftScrollbarDelegate.changedActiveSpointFromTo(this.activeSpoint, num);
						}
						this.activeSpoint = num;
					}
					return;
				}
			}
			if (this.lift.x >= this.spointsLimits[this.spointsNum - 1].x && this.activeSpoint != this.limitPoints[this.spointsNum - 1])
			{
				if (this.delegateLiftScrollbarDelegate != null)
				{
					this.delegateLiftScrollbarDelegate.changedActiveSpointFromTo(this.activeSpoint, this.limitPoints[this.spointsNum - 1]);
				}
				this.activeSpoint = this.limitPoints[this.spointsNum - 1];
			}
		}

		// Token: 0x06000633 RID: 1587 RVA: 0x000332B2 File Offset: 0x000314B2
		public override void dealloc()
		{
			this.spoints = null;
			this.spointsLimits = null;
			this.limitPoints = null;
			this.container = null;
			this.delegateLiftScrollbarDelegate = null;
			base.dealloc();
		}

		// Token: 0x06000634 RID: 1588 RVA: 0x000332DD File Offset: 0x000314DD
		public override bool onTouchDownXY(float tx, float ty)
		{
			return base.onTouchDownXY(tx, ty);
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x000332E7 File Offset: 0x000314E7
		public override bool onTouchUpXY(float tx, float ty)
		{
			bool flag = base.onTouchUpXY(tx, ty);
			this.container.startMovingToSpointInDirection(MathHelper.vectZero);
			return flag;
		}

		// Token: 0x06000636 RID: 1590 RVA: 0x00033304 File Offset: 0x00031504
		public void percentXY(float px, float py)
		{
			Vector maxScroll = this.container.getMaxScroll();
			this.container.setScroll(MathHelper.vect(maxScroll.x * px, maxScroll.y * py));
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x00033340 File Offset: 0x00031540
		public virtual void updateLift()
		{
			Vector scroll = this.container.getScroll();
			Vector maxScroll = this.container.getMaxScroll();
			float num = 0f;
			float num2 = 0f;
			if (maxScroll.x != 0f)
			{
				num = scroll.x / maxScroll.x;
			}
			if (maxScroll.y != 0f)
			{
				num2 = scroll.y / maxScroll.y;
			}
			this.lift.x = (this.lift.maxX - this.lift.minX) * num + this.lift.minX;
			this.lift.y = (this.lift.maxY - this.lift.minY) * num2 + this.lift.minY;
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x00033408 File Offset: 0x00031608
		public virtual void calcScrollPoints()
		{
			Vector maxScroll = this.container.getMaxScroll();
			this.spointsNum = this.container.getTotalScrollPoints();
			this.spoints = null;
			this.spointsLimits = null;
			this.limitPoints = null;
			this.spoints = new Vector[this.spointsNum];
			this.spointsLimits = new Vector[this.spointsNum];
			this.limitPoints = new int[this.spointsNum];
			for (int i = 0; i < this.spointsNum; i++)
			{
				Vector vector = MathHelper.vectNeg(this.container.getScrollPoint(i));
				float num = 0f;
				float num2 = 0f;
				if (maxScroll.x != 0f)
				{
					num = vector.x / maxScroll.x;
				}
				if (maxScroll.y != 0f)
				{
					num2 = vector.y / maxScroll.y;
				}
				float num3 = (this.lift.maxX - this.lift.minX) * num + this.lift.minX;
				float num4 = (this.lift.maxY - this.lift.minY) * num2 + this.lift.minY;
				this.spoints[i] = MathHelper.vect(num3, num4);
			}
			for (int j = 0; j < this.spointsNum; j++)
			{
				this.spointsLimits[j] = this.spoints[j];
				this.limitPoints[j] = j;
			}
			bool flag = true;
			while (flag)
			{
				flag = false;
				for (int k = 0; k < this.spointsNum - 1; k++)
				{
					if (this.spointsLimits[k].x > this.spointsLimits[k + 1].x)
					{
						flag = true;
						Vector vector2 = this.spointsLimits[k];
						this.spointsLimits[k] = this.spointsLimits[k + 1];
						this.spointsLimits[k + 1] = vector2;
						int num5 = this.limitPoints[k];
						this.limitPoints[k] = this.limitPoints[k + 1];
						this.limitPoints[k + 1] = num5;
					}
				}
			}
			for (int l = 0; l < this.spointsNum - 1; l++)
			{
				Vector vector3 = this.spointsLimits[l];
				Vector vector4 = this.spointsLimits[l + 1];
				Vector[] array = this.spointsLimits;
				int num6 = l;
				array[num6].x = array[num6].x + (vector4.x - vector3.x) / 2f;
			}
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x000336A8 File Offset: 0x000318A8
		public virtual void setContainer(ScrollableContainer c)
		{
			this.container = c;
			if (this.container != null)
			{
				this.calcScrollPoints();
				this.updateLift();
			}
		}

		// Token: 0x04000857 RID: 2135
		public Vector[] spoints;

		// Token: 0x04000858 RID: 2136
		public Vector[] spointsLimits;

		// Token: 0x04000859 RID: 2137
		public int[] limitPoints;

		// Token: 0x0400085A RID: 2138
		public int spointsNum;

		// Token: 0x0400085B RID: 2139
		public int activeSpoint;

		// Token: 0x0400085C RID: 2140
		public Lift lift;

		// Token: 0x0400085D RID: 2141
		public ScrollableContainer container;

		// Token: 0x0400085E RID: 2142
		public LiftScrollbarDelegate delegateLiftScrollbarDelegate;
	}
}
