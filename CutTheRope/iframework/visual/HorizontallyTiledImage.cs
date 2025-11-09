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
                    tiles[i] = -1;
                }
                align = 18;
            }
            return this;
        }

        public override void draw()
        {
            preDraw();
            float w = texture.quadRects[tiles[0]].w;
            float w2 = texture.quadRects[tiles[2]].w;
            float num = width - (w + w2);
            if (num >= 0f)
            {
                GLDrawer.drawImageQuad(texture, tiles[0], drawX, drawY + offsets[0]);
                GLDrawer.drawImageTiledCool(texture, tiles[1], drawX + w, drawY + offsets[1], num, texture.quadRects[tiles[1]].h);
                GLDrawer.drawImageQuad(texture, tiles[2], drawX + w + num, drawY + offsets[2]);
            }
            else
            {
                CTRRectangle r = texture.quadRects[tiles[0]];
                CTRRectangle r2 = texture.quadRects[tiles[2]];
                r.w = Math.Min(r.w, width / 2f);
                r2.w = Math.Min(r2.w, width - r.w);
                r2.x += texture.quadRects[tiles[2]].w - r2.w;
                GLDrawer.drawImagePart(texture, r, drawX, drawY + offsets[0]);
                GLDrawer.drawImagePart(texture, r2, drawX + r.w, drawY + offsets[2]);
            }
            postDraw();
        }

        public virtual void setTileHorizontallyLeftCenterRight(int l, int c, int r)
        {
            tiles[0] = l;
            tiles[1] = c;
            tiles[2] = r;
            float h = texture.quadRects[tiles[0]].h;
            float h2 = texture.quadRects[tiles[1]].h;
            float h3 = texture.quadRects[tiles[2]].h;
            if (h >= h2 && h >= h3)
            {
                height = (int)h;
            }
            else if (h2 >= h && h2 >= h3)
            {
                height = (int)h2;
            }
            else
            {
                height = (int)h3;
            }
            offsets[0] = (height - h) / 2f;
            offsets[1] = (height - h2) / 2f;
            offsets[2] = (height - h3) / 2f;
        }

        public static HorizontallyTiledImage HorizontallyTiledImage_create(CTRTexture2D t)
        {
            return (HorizontallyTiledImage)new HorizontallyTiledImage().initWithTexture(t);
        }

        public static HorizontallyTiledImage HorizontallyTiledImage_createWithResID(int r)
        {
            return HorizontallyTiledImage_create(Application.getTexture(r));
        }

        public static HorizontallyTiledImage HorizontallyTiledImage_createWithResIDQuad(int r, int q)
        {
            HorizontallyTiledImage horizontallyTiledImage = HorizontallyTiledImage_create(Application.getTexture(r));
            horizontallyTiledImage.setDrawQuad(q);
            return horizontallyTiledImage;
        }

        public int[] tiles = new int[3];

        public float[] offsets = new float[3];

        public int align;
    }
}
