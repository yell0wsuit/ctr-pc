using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000048 RID: 72
	internal class Scrollbar : BaseElement
	{
		// Token: 0x06000260 RID: 608 RVA: 0x0000D3B5 File Offset: 0x0000B5B5
		public override void update(float delta)
		{
			base.update(delta);
			if (this.delegateProvider != null)
			{
				this.delegateProvider(ref this.sp, ref this.mp, ref this.sc);
			}
		}

		// Token: 0x06000261 RID: 609 RVA: 0x0000D3E4 File Offset: 0x0000B5E4
		public override void draw()
		{
			base.preDraw();
			if (CutTheRope.iframework.helpers.MathHelper.vectEqual(this.sp, CutTheRope.iframework.helpers.MathHelper.vectUndefined) && this.delegateProvider != null)
			{
				this.delegateProvider(ref this.sp, ref this.mp, ref this.sc);
			}
			OpenGL.glDisable(0);
			bool flag = false;
			float num;
			float num2;
			float num3;
			float num5;
			if (this.vertical)
			{
				num = (float)this.width - 2f;
				num2 = 1f;
				num3 = (float)Math.Round(((double)this.height - 2.0) / (double)this.sc.y);
				float num4 = ((this.mp.y != 0f) ? (this.sp.y / this.mp.y) : 1f);
				num5 = (float)(1.0 + ((double)this.height - 2.0 - (double)num3) * (double)num4);
				if (num3 > (float)this.height)
				{
					flag = true;
				}
			}
			else
			{
				num3 = (float)this.height - 2f;
				num5 = 1f;
				num = (float)Math.Round(((double)this.width - 2.0) / (double)this.sc.x);
				float num6 = ((this.mp.x != 0f) ? (this.sp.x / this.mp.x) : 1f);
				num2 = (float)(1.0 + ((double)this.width - 2.0 - (double)num) * (double)num6);
				if (num > (float)this.width)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				GLDrawer.drawSolidRectWOBorder(this.drawX, this.drawY, (float)this.width, (float)this.height, this.backColor);
				GLDrawer.drawSolidRectWOBorder(this.drawX + num2, this.drawY + num5, num, num3, this.scrollerColor);
			}
			OpenGL.glEnable(0);
			OpenGL.glColor4f(Color.White);
			base.postDraw();
		}

		// Token: 0x06000262 RID: 610 RVA: 0x0000D5E0 File Offset: 0x0000B7E0
		public virtual Scrollbar initWithWidthHeightVertical(float w, float h, bool v)
		{
			if (this.init() != null)
			{
				this.width = (int)w;
				this.height = (int)h;
				this.vertical = v;
				this.sp = CutTheRope.iframework.helpers.MathHelper.vectUndefined;
				this.mp = CutTheRope.iframework.helpers.MathHelper.vectUndefined;
				this.sc = CutTheRope.iframework.helpers.MathHelper.vectUndefined;
				this.backColor = RGBAColor.MakeRGBA(1f, 1f, 1f, 0.5f);
				this.scrollerColor = RGBAColor.MakeRGBA(0f, 0f, 0f, 0.5f);
			}
			return this;
		}

		// Token: 0x040001C5 RID: 453
		public Vector sp;

		// Token: 0x040001C6 RID: 454
		public Vector mp;

		// Token: 0x040001C7 RID: 455
		public Vector sc;

		// Token: 0x040001C8 RID: 456
		public Scrollbar.ProvideScrollPosMaxScrollPosScrollCoeff delegateProvider;

		// Token: 0x040001C9 RID: 457
		public bool vertical;

		// Token: 0x040001CA RID: 458
		public RGBAColor backColor;

		// Token: 0x040001CB RID: 459
		public RGBAColor scrollerColor;

		// Token: 0x020000B0 RID: 176
		// (Invoke) Token: 0x0600066C RID: 1644
		public delegate void ProvideScrollPosMaxScrollPosScrollCoeff(ref Vector sp, ref Vector mp, ref Vector sc);
	}
}
