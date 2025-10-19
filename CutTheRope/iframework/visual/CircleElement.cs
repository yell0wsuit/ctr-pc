using System;
using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200002E RID: 46
	internal class CircleElement : BaseElement
	{
		// Token: 0x0600019C RID: 412 RVA: 0x000085F2 File Offset: 0x000067F2
		public override NSObject init()
		{
			if (base.init() != null)
			{
				this.vertextCount = 32;
				this.solid = true;
			}
			return this;
		}

		// Token: 0x0600019D RID: 413 RVA: 0x0000860C File Offset: 0x0000680C
		public override void draw()
		{
			base.preDraw();
			OpenGL.glDisable(0);
			CutTheRope.iframework.helpers.MathHelper.MIN(this.width, this.height);
			bool flag = this.solid;
			OpenGL.glEnable(0);
			OpenGL.glColor4f(Color.White);
			base.postDraw();
		}

		// Token: 0x0400012B RID: 299
		public bool solid;

		// Token: 0x0400012C RID: 300
		public int vertextCount;
	}
}
