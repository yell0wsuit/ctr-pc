using System;
using CutTheRope.iframework.core;

namespace CutTheRope.iframework.visual
{
	// Token: 0x0200004B RID: 75
	internal class TiledImage : Image
	{
		// Token: 0x0600028E RID: 654 RVA: 0x0000E6A5 File Offset: 0x0000C8A5
		public virtual void setTile(int t)
		{
			this.q = t;
		}

		// Token: 0x0600028F RID: 655 RVA: 0x0000E6AE File Offset: 0x0000C8AE
		public override void draw()
		{
			this.preDraw();
			GLDrawer.drawImageTiled(this.texture, this.q, this.drawX, this.drawY, (float)this.width, (float)this.height);
			this.postDraw();
		}

		// Token: 0x06000290 RID: 656 RVA: 0x0000E6E7 File Offset: 0x0000C8E7
		private static TiledImage TiledImage_create(Texture2D t)
		{
			return (TiledImage)new TiledImage().initWithTexture(t);
		}

		// Token: 0x06000291 RID: 657 RVA: 0x0000E6F9 File Offset: 0x0000C8F9
		public static TiledImage TiledImage_createWithResID(int r)
		{
			return TiledImage.TiledImage_create(Application.getTexture(r));
		}

		// Token: 0x06000292 RID: 658 RVA: 0x0000E706 File Offset: 0x0000C906
		private static TiledImage TiledImage_createWithResIDQuad(int r, int q)
		{
			TiledImage tiledImage = TiledImage.TiledImage_createWithResID(r);
			tiledImage.setDrawQuad(q);
			return tiledImage;
		}

		// Token: 0x040001F8 RID: 504
		private int q;
	}
}
