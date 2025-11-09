using System;

namespace CutTheRope.iframework.visual
{
    internal class VerticallyTiledImage : Image
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
            float h = this.texture.quadRects[this.tiles[0]].h;
            float h2 = this.texture.quadRects[this.tiles[2]].h;
            float num = (float)this.height - (h + h2);
            if (num >= 0f)
            {
                GLDrawer.drawImageQuad(this.texture, this.tiles[0], this.drawX + this.offsets[0], this.drawY);
                GLDrawer.drawImageTiledCool(this.texture, this.tiles[1], this.drawX + this.offsets[1], this.drawY + h, (float)this.width, num);
                GLDrawer.drawImageQuad(this.texture, this.tiles[2], this.drawX + this.offsets[2], this.drawY + h + num);
            }
            else
            {
                Rectangle r = this.texture.quadRects[this.tiles[0]];
                Rectangle r2 = this.texture.quadRects[this.tiles[2]];
                r.h = Math.Min(r.h, (float)this.height / 2f);
                r2.h = Math.Min(r2.h, (float)this.height - r.h);
                r2.y += this.texture.quadRects[this.tiles[2]].h - r2.h;
                GLDrawer.drawImagePart(this.texture, r, this.drawX + this.offsets[0], this.drawY);
                GLDrawer.drawImagePart(this.texture, r2, this.drawX + this.offsets[2], this.drawY + r.h);
            }
            this.postDraw();
        }

        public virtual void setTileVerticallyTopCenterBottom(int t, int c, int b)
        {
            this.tiles[0] = t;
            this.tiles[1] = c;
            this.tiles[2] = b;
            float w = this.texture.quadRects[this.tiles[0]].w;
            float w2 = this.texture.quadRects[this.tiles[1]].w;
            float w3 = this.texture.quadRects[this.tiles[2]].w;
            if (w >= w2 && w >= w3)
            {
                this.width = (int)w;
            }
            else if (w2 >= w && w2 >= w3)
            {
                this.width = (int)w2;
            }
            else
            {
                this.width = (int)w3;
            }
            this.offsets[0] = ((float)this.width - w) / 2f;
            this.offsets[1] = ((float)this.width - w2) / 2f;
            this.offsets[2] = ((float)this.width - w3) / 2f;
        }

        public int[] tiles = new int[3];

        public float[] offsets = new float[3];

        public int align;
    }
}
