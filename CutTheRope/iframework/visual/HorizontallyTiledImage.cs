using CutTheRope.iframework.core;
using System;

namespace CutTheRope.iframework.visual
{
    internal class HorizontallyTiledImage : Image
    {
        public override Image initWithTexture(CTRTexture2D t)
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
                CTRRectangle r = this.texture.quadRects[this.tiles[0]];
                CTRRectangle r2 = this.texture.quadRects[this.tiles[2]];
                r.w = Math.Min(r.w, (float)this.width / 2f);
                r2.w = Math.Min(r2.w, (float)this.width - r.w);
                r2.x += this.texture.quadRects[this.tiles[2]].w - r2.w;
                GLDrawer.drawImagePart(this.texture, r, this.drawX, this.drawY + this.offsets[0]);
                GLDrawer.drawImagePart(this.texture, r2, this.drawX + r.w, this.drawY + this.offsets[2]);
            }
            this.postDraw();
        }

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

        public static HorizontallyTiledImage HorizontallyTiledImage_create(CTRTexture2D t)
        {
            return (HorizontallyTiledImage)new HorizontallyTiledImage().initWithTexture(t);
        }

        public static HorizontallyTiledImage HorizontallyTiledImage_createWithResID(int r)
        {
            return HorizontallyTiledImage.HorizontallyTiledImage_create(Application.getTexture(r));
        }

        public static HorizontallyTiledImage HorizontallyTiledImage_createWithResIDQuad(int r, int q)
        {
            HorizontallyTiledImage horizontallyTiledImage = HorizontallyTiledImage.HorizontallyTiledImage_create(Application.getTexture(r));
            horizontallyTiledImage.setDrawQuad(q);
            return horizontallyTiledImage;
        }

        public int[] tiles = new int[3];

        public float[] offsets = new float[3];

        public int align;
    }
}
