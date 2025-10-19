using System;
using System.Collections.Generic;
using CutTheRope.iframework.core;
using CutTheRope.ios;

namespace CutTheRope.iframework.helpers
{
	// Token: 0x02000063 RID: 99
	internal class Mover : NSObject
	{
		// Token: 0x06000391 RID: 913 RVA: 0x00014490 File Offset: 0x00012690
		public virtual Mover initWithPathCapacityMoveSpeedRotateSpeed(int l, float m_, float r_)
		{
			int num = (int)m_;
			int num2 = (int)r_;
			if (base.init() != null)
			{
				this.pathLen = 0;
				this.pathCapacity = l;
				this.rotateSpeed = (float)num2;
				if (this.pathCapacity > 0)
				{
					this.path = new Vector[this.pathCapacity];
					for (int i = 0; i < this.path.Length; i++)
					{
						this.path[i] = default(Vector);
					}
					this.moveSpeed = new float[this.pathCapacity];
					for (int j = 0; j < this.moveSpeed.Length; j++)
					{
						this.moveSpeed[j] = (float)num;
					}
				}
				this.paused = false;
			}
			return this;
		}

		// Token: 0x06000392 RID: 914 RVA: 0x0001453C File Offset: 0x0001273C
		public virtual void setMoveSpeed(float ms)
		{
			for (int i = 0; i < this.pathCapacity; i++)
			{
				this.moveSpeed[i] = ms;
			}
		}

		// Token: 0x06000393 RID: 915 RVA: 0x00014564 File Offset: 0x00012764
		public virtual void setPathFromStringandStart(NSString p, Vector s)
		{
			if (p.characterAtIndex(0) == 'R')
			{
				bool flag = p.characterAtIndex(1) == 'C';
				int num = p.substringFromIndex(2).intValue();
				int num2 = num / 2;
				float num3 = (float)(6.283185307179586 / (double)num2);
				if (!flag)
				{
					num3 = 0f - num3;
				}
				float num4 = 0f;
				for (int i = 0; i < num2; i++)
				{
					float x = s.x + (float)num * (float)Math.Cos((double)num4);
					float y = s.y + (float)num * (float)Math.Sin((double)num4);
					this.addPathPoint(MathHelper.vect(x, y));
					num4 += num3;
				}
				return;
			}
			this.addPathPoint(s);
			if (p.characterAtIndex(p.length() - 1) == ',')
			{
				p = p.substringToIndex(p.length() - 1);
			}
			List<NSString> list = p.componentsSeparatedByString(',');
			for (int j = 0; j < list.Count; j += 2)
			{
				NSString nSString2 = list[j];
				NSString nSString3 = list[j + 1];
				this.addPathPoint(MathHelper.vect(s.x + nSString2.floatValue(), s.y + nSString3.floatValue()));
			}
		}

		// Token: 0x06000394 RID: 916 RVA: 0x00014690 File Offset: 0x00012890
		public virtual void addPathPoint(Vector v)
		{
			Vector[] array = this.path;
			int num = this.pathLen;
			this.pathLen = num + 1;
			array[num] = v;
		}

		// Token: 0x06000395 RID: 917 RVA: 0x000146BA File Offset: 0x000128BA
		public virtual void start()
		{
			if (this.pathLen > 0)
			{
				this.pos = this.path[0];
				this.targetPoint = 1;
				this.calculateOffset();
			}
		}

		// Token: 0x06000396 RID: 918 RVA: 0x000146E4 File Offset: 0x000128E4
		public virtual void pause()
		{
			this.paused = true;
		}

		// Token: 0x06000397 RID: 919 RVA: 0x000146ED File Offset: 0x000128ED
		public virtual void unpause()
		{
			this.paused = false;
		}

		// Token: 0x06000398 RID: 920 RVA: 0x000146F6 File Offset: 0x000128F6
		public virtual void setRotateSpeed(float rs)
		{
			this.rotateSpeed = rs;
		}

		// Token: 0x06000399 RID: 921 RVA: 0x000146FF File Offset: 0x000128FF
		public virtual void jumpToPoint(int p)
		{
			this.targetPoint = p;
			this.pos = this.path[this.targetPoint];
			this.calculateOffset();
		}

		// Token: 0x0600039A RID: 922 RVA: 0x00014728 File Offset: 0x00012928
		public virtual void calculateOffset()
		{
			Vector v = this.path[this.targetPoint];
			this.offset = MathHelper.vectMult(MathHelper.vectNormalize(MathHelper.vectSub(v, this.pos)), this.moveSpeed[this.targetPoint]);
		}

		// Token: 0x0600039B RID: 923 RVA: 0x00014770 File Offset: 0x00012970
		public virtual void setMoveSpeedforPoint(float ms, int i)
		{
			this.moveSpeed[i] = ms;
		}

		// Token: 0x0600039C RID: 924 RVA: 0x0001477B File Offset: 0x0001297B
		public virtual void setMoveReverse(bool r)
		{
			this.reverse = r;
		}

		// Token: 0x0600039D RID: 925 RVA: 0x00014784 File Offset: 0x00012984
		public virtual void update(float delta)
		{
			if (this.paused)
			{
				return;
			}
			if (this.pathLen > 0)
			{
				Vector v = this.path[this.targetPoint];
				bool flag = false;
				if (!MathHelper.vectEqual(this.pos, v))
				{
					float num = delta;
					if (this.overrun != 0f)
					{
						num += this.overrun;
						this.overrun = 0f;
					}
					this.pos = MathHelper.vectAdd(this.pos, MathHelper.vectMult(this.offset, num));
					if (!MathHelper.sameSign(this.offset.x, v.x - this.pos.x) || !MathHelper.sameSign(this.offset.y, v.y - this.pos.y))
					{
						this.overrun = MathHelper.vectLength(MathHelper.vectSub(this.pos, v));
						float num2 = MathHelper.vectLength(this.offset);
						this.overrun /= num2;
						this.pos = v;
						flag = true;
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					if (this.reverse)
					{
						this.targetPoint--;
						if (this.targetPoint < 0)
						{
							this.targetPoint = this.pathLen - 1;
						}
					}
					else
					{
						this.targetPoint++;
						if (this.targetPoint >= this.pathLen)
						{
							this.targetPoint = 0;
						}
					}
					this.calculateOffset();
				}
			}
			if (this.rotateSpeed != 0f)
			{
				if (this.use_angle_initial && this.targetPoint == 0)
				{
					this.angle_ = this.angle_initial;
					return;
				}
				this.angle_ += (double)(this.rotateSpeed * delta);
			}
		}

		// Token: 0x0600039E RID: 926 RVA: 0x0001492C File Offset: 0x00012B2C
		public override void dealloc()
		{
			this.path = null;
			this.moveSpeed = null;
			base.dealloc();
		}

		// Token: 0x0600039F RID: 927 RVA: 0x00014942 File Offset: 0x00012B42
		public static bool moveVariableToTarget(ref float v, double t, double speed, double delta)
		{
			return Mover.moveVariableToTarget(ref v, (float)t, (float)speed, (float)delta);
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x00014950 File Offset: 0x00012B50
		public static bool moveVariableToTarget(ref float v, float t, float speed, float delta)
		{
			if (t != v)
			{
				if (t > v)
				{
					v += speed * delta;
					if (v > t)
					{
						v = t;
					}
				}
				else
				{
					v -= speed * delta;
					if (v < t)
					{
						v = t;
					}
				}
				if (t == v)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0400028C RID: 652
		private float[] moveSpeed;

		// Token: 0x0400028D RID: 653
		private float rotateSpeed;

		// Token: 0x0400028E RID: 654
		public Vector[] path;

		// Token: 0x0400028F RID: 655
		public int pathLen;

		// Token: 0x04000290 RID: 656
		private int pathCapacity;

		// Token: 0x04000291 RID: 657
		public Vector pos;

		// Token: 0x04000292 RID: 658
		public double angle_;

		// Token: 0x04000293 RID: 659
		public double angle_initial;

		// Token: 0x04000294 RID: 660
		public bool use_angle_initial;

		// Token: 0x04000295 RID: 661
		private bool paused;

		// Token: 0x04000296 RID: 662
		public int targetPoint;

		// Token: 0x04000297 RID: 663
		private bool reverse;

		// Token: 0x04000298 RID: 664
		private float overrun;

		// Token: 0x04000299 RID: 665
		private Vector offset;
	}
}
