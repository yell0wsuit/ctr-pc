using System;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
	// Token: 0x02000070 RID: 112
	internal class Bubble : GameObject
	{
		// Token: 0x0600045A RID: 1114 RVA: 0x00018630 File Offset: 0x00016830
		public static Bubble Bubble_create(Texture2D t)
		{
			return (Bubble)new Bubble().initWithTexture(t);
		}

		// Token: 0x0600045B RID: 1115 RVA: 0x00018642 File Offset: 0x00016842
		public static Bubble Bubble_createWithResID(int r)
		{
			return Bubble.Bubble_create(Application.getTexture(r));
		}

		// Token: 0x0600045C RID: 1116 RVA: 0x0001864F File Offset: 0x0001684F
		public static Bubble Bubble_createWithResIDQuad(int r, int q)
		{
			Bubble bubble = Bubble.Bubble_create(Application.getTexture(r));
			bubble.setDrawQuad(q);
			return bubble;
		}

		// Token: 0x0600045D RID: 1117 RVA: 0x00018664 File Offset: 0x00016864
		public override void draw()
		{
			base.preDraw();
			if (!this.withoutShadow)
			{
				if (this.quadToDraw == -1)
				{
					GLDrawer.drawImage(this.texture, this.drawX, this.drawY);
				}
				else
				{
					this.drawQuad(this.quadToDraw);
				}
			}
			base.postDraw();
		}

		// Token: 0x040002F7 RID: 759
		public bool popped;

		// Token: 0x040002F8 RID: 760
		public float initial_rotation;

		// Token: 0x040002F9 RID: 761
		public float initial_x;

		// Token: 0x040002FA RID: 762
		public float initial_y;

		// Token: 0x040002FB RID: 763
		public RotatedCircle initial_rotatedCircle;

		// Token: 0x040002FC RID: 764
		public bool withoutShadow;
	}
}
