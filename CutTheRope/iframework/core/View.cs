using System;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.core
{
	// Token: 0x0200006C RID: 108
	internal class View : BaseElement
	{
		// Token: 0x0600041A RID: 1050 RVA: 0x0001624B File Offset: 0x0001444B
		public virtual NSObject initFullscreen()
		{
			if (base.init() != null)
			{
				this.width = (int)FrameworkTypes.SCREEN_WIDTH;
				this.height = (int)FrameworkTypes.SCREEN_HEIGHT;
			}
			return this;
		}

		// Token: 0x0600041B RID: 1051 RVA: 0x0001626E File Offset: 0x0001446E
		public override NSObject init()
		{
			return this.initFullscreen();
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x00016276 File Offset: 0x00014476
		public override void draw()
		{
			OpenGL.glColor4f(Color.White);
			OpenGL.glEnable(0);
			OpenGL.glEnable(1);
			OpenGL.glBlendFunc(BlendingFactor.GL_SRC_ALPHA, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			base.preDraw();
			base.postDraw();
			OpenGL.glDisable(0);
			OpenGL.glDisable(1);
		}
	}
}
