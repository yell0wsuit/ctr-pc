using System;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000035 RID: 53
	internal class HBox : BaseElement
	{
		// Token: 0x060001DA RID: 474 RVA: 0x000095E8 File Offset: 0x000077E8
		public override int addChildwithID(BaseElement c, int i)
		{
			int num = base.addChildwithID(c, i);
			if (this.align == 8)
			{
				c.anchor = (c.parentAnchor = 9);
			}
			else if (this.align == 16)
			{
				c.anchor = (c.parentAnchor = 17);
			}
			else if (this.align == 32)
			{
				c.anchor = (c.parentAnchor = 33);
			}
			c.x = this.nextElementX;
			this.nextElementX += (float)c.width + this.offset;
			this.width = (int)(this.nextElementX - this.offset);
			return num;
		}

		// Token: 0x060001DB RID: 475 RVA: 0x0000968C File Offset: 0x0000788C
		public virtual HBox initWithOffsetAlignHeight(double of, int a, double h)
		{
			return this.initWithOffsetAlignHeight((float)of, a, (float)h);
		}

		// Token: 0x060001DC RID: 476 RVA: 0x00009699 File Offset: 0x00007899
		public virtual HBox initWithOffsetAlignHeight(float of, int a, float h)
		{
			if (this.init() != null)
			{
				this.offset = of;
				this.align = a;
				this.nextElementX = 0f;
				this.height = (int)h;
			}
			return this;
		}

		// Token: 0x0400013A RID: 314
		public float offset;

		// Token: 0x0400013B RID: 315
		public int align;

		// Token: 0x0400013C RID: 316
		public float nextElementX;
	}
}
