using System;
using CutTheRope.iframework.visual;

namespace CutTheRope.ctr_commons
{
	// Token: 0x0200009D RID: 157
	internal class Lift : Button
	{
		// Token: 0x0600063B RID: 1595 RVA: 0x000336CD File Offset: 0x000318CD
		public override bool onTouchDownXY(float tx, float ty)
		{
			this.startX = tx - this.x;
			this.startY = ty - this.y;
			return base.onTouchDownXY(tx, ty);
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x000336F3 File Offset: 0x000318F3
		public override bool onTouchUpXY(float tx, float ty)
		{
			this.startX = 0f;
			this.startY = 0f;
			return base.onTouchUpXY(tx, ty);
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x00033714 File Offset: 0x00031914
		public override bool onTouchMoveXY(float tx, float ty)
		{
			if (this.state == Button.BUTTON_STATE.BUTTON_DOWN)
			{
				this.x = Math.Max(Math.Min(tx - this.startX, this.maxX), this.minX);
				this.y = Math.Max(Math.Min(ty - this.startY, this.maxY), this.minY);
				if (this.maxX != 0f)
				{
					float num = (this.x - this.minX) / (this.maxX - this.minX);
					if (num != this.xPercent)
					{
						this.xPercent = num;
						if (this.liftDelegate != null)
						{
							this.liftDelegate(this.xPercent, this.yPercent);
						}
					}
				}
				if (this.maxY != 0f)
				{
					float num2 = (this.y - this.minY) / (this.maxY - this.minY);
					if (num2 != this.yPercent)
					{
						this.yPercent = num2;
						if (this.liftDelegate != null)
						{
							this.liftDelegate(this.xPercent, this.yPercent);
						}
					}
				}
				return true;
			}
			return base.onTouchMoveXY(tx, ty);
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x0003382F File Offset: 0x00031A2F
		public override void dealloc()
		{
			this.liftDelegate = null;
			base.dealloc();
		}

		// Token: 0x0400085F RID: 2143
		public float startX;

		// Token: 0x04000860 RID: 2144
		public float startY;

		// Token: 0x04000861 RID: 2145
		public Lift.PercentXY liftDelegate;

		// Token: 0x04000862 RID: 2146
		public float minX;

		// Token: 0x04000863 RID: 2147
		public float maxX;

		// Token: 0x04000864 RID: 2148
		public float minY;

		// Token: 0x04000865 RID: 2149
		public float maxY;

		// Token: 0x04000866 RID: 2150
		public float xPercent;

		// Token: 0x04000867 RID: 2151
		public float yPercent;

		// Token: 0x020000CF RID: 207
		// (Invoke) Token: 0x0600068D RID: 1677
		public delegate void PercentXY(float px, float py);
	}
}
