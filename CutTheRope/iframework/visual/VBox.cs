using System;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000053 RID: 83
	internal class VBox : BaseElement
	{
		// Token: 0x060002C6 RID: 710 RVA: 0x00010FE0 File Offset: 0x0000F1E0
		public override int addChildwithID(BaseElement c, int i)
		{
			int num = base.addChildwithID(c, i);
			if (this.align == 1)
			{
				c.anchor = (c.parentAnchor = 9);
			}
			else if (this.align == 4)
			{
				c.anchor = (c.parentAnchor = 12);
			}
			else if (this.align == 2)
			{
				c.anchor = (c.parentAnchor = 10);
			}
			c.y = this.nextElementY;
			this.nextElementY += (float)c.height + this.offset;
			this.height = (int)(this.nextElementY - this.offset);
			return num;
		}

		// Token: 0x060002C7 RID: 711 RVA: 0x00011082 File Offset: 0x0000F282
		public virtual VBox initWithOffsetAlignWidth(double of, int a, double w)
		{
			return this.initWithOffsetAlignWidth((float)of, a, (float)w);
		}

		// Token: 0x060002C8 RID: 712 RVA: 0x0001108F File Offset: 0x0000F28F
		public virtual VBox initWithOffsetAlignWidth(float of, int a, float w)
		{
			if (this.init() != null)
			{
				this.offset = of;
				this.align = a;
				this.nextElementY = 0f;
				this.width = (int)w;
			}
			return this;
		}

		// Token: 0x0400022E RID: 558
		public float offset;

		// Token: 0x0400022F RID: 559
		public int align;

		// Token: 0x04000230 RID: 560
		public float nextElementY;
	}
}
