using System;
using CutTheRope.iframework.core;
using CutTheRope.ios;
using CutTheRope.windows;

namespace CutTheRope.iframework.helpers
{
	// Token: 0x0200005D RID: 93
	internal class Camera2D : NSObject
	{
		// Token: 0x0600032A RID: 810 RVA: 0x00012854 File Offset: 0x00010A54
		public virtual Camera2D initWithSpeedandType(float s, CAMERA_TYPE t)
		{
			if (base.init() != null)
			{
				this.speed = s;
				this.type = t;
			}
			return this;
		}

		// Token: 0x0600032B RID: 811 RVA: 0x00012870 File Offset: 0x00010A70
		public virtual void moveToXYImmediate(float x, float y, bool immediate)
		{
			this.target.x = x;
			this.target.y = y;
			if (immediate)
			{
				this.pos = this.target;
				return;
			}
			if (this.type == CAMERA_TYPE.CAMERA_SPEED_DELAY)
			{
				this.offset = MathHelper.vectMult(MathHelper.vectSub(this.target, this.pos), this.speed);
				return;
			}
			if (this.type == CAMERA_TYPE.CAMERA_SPEED_PIXELS)
			{
				this.offset = MathHelper.vectMult(MathHelper.vectNormalize(MathHelper.vectSub(this.target, this.pos)), this.speed);
			}
		}

		// Token: 0x0600032C RID: 812 RVA: 0x00012900 File Offset: 0x00010B00
		public virtual void update(float delta)
		{
			if (!MathHelper.vectEqual(this.pos, this.target))
			{
				this.pos = MathHelper.vectAdd(this.pos, MathHelper.vectMult(this.offset, delta));
				this.pos = MathHelper.vect(MathHelper.round((double)this.pos.x), MathHelper.round((double)this.pos.y));
				if (!MathHelper.sameSign(this.offset.x, this.target.x - this.pos.x) || !MathHelper.sameSign(this.offset.y, this.target.y - this.pos.y))
				{
					this.pos = this.target;
				}
			}
		}

		// Token: 0x0600032D RID: 813 RVA: 0x000129CB File Offset: 0x00010BCB
		public virtual void applyCameraTransformation()
		{
			OpenGL.glTranslatef((double)(0f - this.pos.x), (double)(0f - this.pos.y), 0.0);
		}

		// Token: 0x0600032E RID: 814 RVA: 0x000129FF File Offset: 0x00010BFF
		public virtual void cancelCameraTransformation()
		{
			OpenGL.glTranslatef((double)this.pos.x, (double)this.pos.y, 0.0);
		}

		// Token: 0x0400026B RID: 619
		public CAMERA_TYPE type;

		// Token: 0x0400026C RID: 620
		public float speed;

		// Token: 0x0400026D RID: 621
		public Vector pos;

		// Token: 0x0400026E RID: 622
		public Vector target;

		// Token: 0x0400026F RID: 623
		public Vector offset;
	}
}
