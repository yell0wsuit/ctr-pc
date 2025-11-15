using System;

namespace CutTheRope.Framework.Visual
{
    internal sealed class VerticallyTiledImage : Image
    {
        public override Image InitWithTexture(CTRTexture2D t)
        {
            if (base.InitWithTexture(t) != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    tiles[i] = -1;
                }
                align = 18;
            }
            return this;
        }

        public override void Draw()
        {
            PreDraw();
            float h = texture.quadRects[tiles[0]].h;
            float h2 = texture.quadRects[tiles[2]].h;
            float num = height - (h + h2);
            if (num >= 0f)
            {
                GLDrawer.DrawImageQuad(texture, tiles[0], drawX + offsets[0], drawY);
                GLDrawer.DrawImageTiledCool(texture, tiles[1], drawX + offsets[1], drawY + h, width, num);
                GLDrawer.DrawImageQuad(texture, tiles[2], drawX + offsets[2], drawY + h + num);
            }
            else
            {
                CTRRectangle r = texture.quadRects[tiles[0]];
                CTRRectangle r2 = texture.quadRects[tiles[2]];
                r.h = Math.Min(r.h, height / 2f);
                r2.h = Math.Min(r2.h, height - r.h);
                r2.y += texture.quadRects[tiles[2]].h - r2.h;
                GLDrawer.DrawImagePart(texture, r, drawX + offsets[0], drawY);
                GLDrawer.DrawImagePart(texture, r2, drawX + offsets[2], drawY + r.h);
            }
            PostDraw();
        }

        public void SetTileVerticallyTopCenterBottom(int t, int c, int b)
        {
            tiles[0] = t;
            tiles[1] = c;
            tiles[2] = b;
            float w = texture.quadRects[tiles[0]].w;
            float w2 = texture.quadRects[tiles[1]].w;
            float w3 = texture.quadRects[tiles[2]].w;
            width = w >= w2 && w >= w3 ? (int)w : w2 >= w && w2 >= w3 ? (int)w2 : (int)w3;
            offsets[0] = (width - w) / 2f;
            offsets[1] = (width - w2) / 2f;
            offsets[2] = (width - w3) / 2f;
        }

        public int[] tiles = new int[3];

        public float[] offsets = new float[3];

        public int align;
    }
}
