using System;
using CutTheRope.iframework.core;

namespace CutTheRope.iframework.visual
{
	// Token: 0x02000036 RID: 54
	internal class HorizontallyTiledImage : Image
	{
		// Token: 0x060001DE RID: 478 RVA: 0x000096D0 File Offset: 0x000078D0
		public override Image initWithTexture(Texture2D t)
		{
			if (base.initWithTexture(t) != null)
			{
				for (int i = 0; i < 3; i++)
				{
					this.tiles[i] = -1;
				}
				this.align = 18;
			}
			return this;
		}

		// Token: 0x060001DF RID: 479 RVA: 0x00009704 File Offset: 0x00007904
		public override void draw()
		{
			this.preDraw();
			float w = this.texture.quadRects[this.tiles[0]].w;
			float w2 = this.texture.quadRects[this.tiles[2]].w;
			float num = (float)this.width - (w + w2);
			if (num >= 0f)
			{
				GLDrawer.drawImageQuad(this.texture, this.tiles[0], this.drawX, this.drawY + this.offsets[0]);
				GLDrawer.drawImageTiledCool(this.texture, this.tiles[1], this.drawX + w, this.drawY + this.offsets[1], num, this.texture.quadRects[this.tiles[1]].h);
				GLDrawer.drawImageQuad(this.texture, this.tiles[2], this.drawX + w + num, this.drawY + this.offsets[2]);
			}
			else
			{
				Rectangle r = this.texture.quadRects[this.tiles[0]];
				Rectangle r2 = this.texture.quadRects[this.tiles[2]];
				r.w = Math.Min(r.w, (float)this.width / 2f);
				r2.w = Math.Min(r2.w, (float)this.width - r.w);
				r2.x += this.texture.quadRects[this.tiles[2]].w - r2.w;
				GLDrawer.drawImagePart(this.texture, r, this.drawX, this.drawY + this.offsets[0]);
				GLDrawer.drawImagePart(this.texture, r2, this.drawX + r.w, this.drawY + this.offsets[2]);
			}
			this.postDraw();
		}

		// Token: 0x060001E0 RID: 480 RVA: 0x00009900 File Offset: 0x00007B00
		public virtual void setTileHorizontallyLeftCenterRight(int l, int c, int r)
		{
			this.tiles[0] = l;
			this.tiles[1] = c;
			this.tiles[2] = r;
			float h = this.texture.quadRects[this.tiles[0]].h;
			float h2 = this.texture.quadRects[this.tiles[1]].h;
			float h3 = this.texture.quadRects[this.tiles[2]].h;
			if (h >= h2 && h >= h3)
			{
				this.height = (int)h;
			}
			else if (h2 >= h && h2 >= h3)
			{
				this.height = (int)h2;
			}
			else
			{
				this.height = (int)h3;
			}
			this.offsets[0] = ((float)this.height - h) / 2f;
			this.offsets[1] = ((float)this.height - h2) / 2f;
			this.offsets[2] = ((float)this.height - h3) / 2f;
		}

		// Token: 0x060001E1 RID: 481 RVA: 0x000099F3 File Offset: 0x00007BF3
		public static HorizontallyTiledImage HorizontallyTiledImage_create(Texture2D t)
		{
			return (HorizontallyTiledImage)new HorizontallyTiledImage().initWithTexture(t);
		}

		// Token: 0x060001E2 RID: 482 RVA: 0x00009A05 File Offset: 0x00007C05
		public static HorizontallyTiledImage HorizontallyTiledImage_createWithResID(int r)
		{
			return HorizontallyTiledImage.HorizontallyTiledImage_create(Application.getTexture(r));
		}

		// Token: 0x060001E3 RID: 483 RVA: 0x00009A12 File Offset: 0x00007C12
		public static HorizontallyTiledImage HorizontallyTiledImage_createWithResIDQuad(int r, int q)
		{
			HorizontallyTiledImage horizontallyTiledImage = HorizontallyTiledImage.HorizontallyTiledImage_create(Application.getTexture(r));
			horizontallyTiledImage.setDrawQuad(q);
			return horizontallyTiledImage;
		}

		// Token: 0x0400013D RID: 317
		public int[] tiles = new int[3];

		// Token: 0x0400013E RID: 318
		public float[] offsets = new float[3];

		// Token: 0x0400013F RID: 319
		public int align;
	}
}
