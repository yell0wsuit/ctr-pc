using System;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000040 RID: 64
	internal class RectangleElement : BaseElement
	{
		// Token: 0x0600022C RID: 556 RVA: 0x0000B35D File Offset: 0x0000955D
		public override NSObject init()
		{
			if (base.init() != null)
			{
				this.solid = true;
			}
			return this;
		}

		// Token: 0x0600022D RID: 557 RVA: 0x0000B370 File Offset: 0x00009570
		public override void draw()
		{
			base.preDraw();
			OpenGL.glDisable(0);
			if (this.solid)
			{
				GLDrawer.drawSolidRectWOBorder(this.drawX, this.drawY, (float)this.width, (float)this.height, this.color);
			}
			else
			{
				GLDrawer.drawRect(this.drawX, this.drawY, (float)this.width, (float)this.height, this.color);
			}
			OpenGL.glEnable(0);
			OpenGL.glColor4f(Color.White);
			base.postDraw();
		}

		// Token: 0x0400018A RID: 394
		public bool solid;
	}
}
