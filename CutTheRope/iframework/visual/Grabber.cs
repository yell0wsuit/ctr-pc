using System;
using CutTheRope.ios;
using CutTheRope.windows;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000034 RID: 52
	internal class Grabber : NSObject
	{
		// Token: 0x060001D5 RID: 469 RVA: 0x000094F4 File Offset: 0x000076F4
		public override NSObject init()
		{
			base.init();
			return this;
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x000094FE File Offset: 0x000076FE
		public override void dealloc()
		{
			base.dealloc();
		}

		// Token: 0x060001D7 RID: 471 RVA: 0x00009506 File Offset: 0x00007706
		public virtual Texture2D grab()
		{
			return (Texture2D)new Texture2D().initFromPixels(0, 0, (int)FrameworkTypes.SCREEN_WIDTH, (int)FrameworkTypes.SCREEN_HEIGHT);
		}

		// Token: 0x060001D8 RID: 472 RVA: 0x00009528 File Offset: 0x00007728
		public static void drawGrabbedImage(Texture2D t, int x, int y)
		{
			if (t != null)
			{
				float[] pointer = new float[] { 0f, 0f, t._maxS, 0f, 0f, t._maxT, t._maxS, t._maxT };
				float[] array = new float[12];
				array[0] = (float)x;
				array[1] = (float)y;
				array[3] = (float)(t._realWidth + x);
				array[4] = (float)y;
				array[6] = (float)x;
				array[7] = (float)(t._realHeight + y);
				array[9] = (float)(t._realWidth + x);
				array[10] = (float)(t._realHeight + y);
				float[] pointer2 = array;
				OpenGL.glEnable(0);
				OpenGL.glBindTexture(t.name());
				OpenGL.glVertexPointer(3, 5, 0, pointer2);
				OpenGL.glTexCoordPointer(2, 5, 0, pointer);
				OpenGL.glDrawArrays(8, 0, 4);
			}
		}
	}
}
