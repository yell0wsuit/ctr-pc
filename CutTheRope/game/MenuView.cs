using System;
using CutTheRope.iframework.core;
using CutTheRope.windows;
using Microsoft.Xna.Framework;

namespace CutTheRope.game
{
	// Token: 0x02000088 RID: 136
	internal class MenuView : View
	{
		// Token: 0x0600059D RID: 1437 RVA: 0x0002E1BC File Offset: 0x0002C3BC
		public override void update(float t)
		{
			Global.MouseCursor.Enable(true);
			base.update(t);
		}

		// Token: 0x0600059E RID: 1438 RVA: 0x0002E1D0 File Offset: 0x0002C3D0
		public override void draw()
		{
			OpenGL.glColor4f(Color.White);
			OpenGL.glEnable(0);
			OpenGL.glEnable(1);
			OpenGL.glBlendFunc(BlendingFactor.GL_ONE, BlendingFactor.GL_ONE_MINUS_SRC_ALPHA);
			base.preDraw();
			base.postDraw();
			OpenGL.glDisable(0);
			OpenGL.glDisable(1);
		}
	}
}
