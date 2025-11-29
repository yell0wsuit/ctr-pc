using System;

using CutTheRope.Framework.Core;
using CutTheRope.GameMain;

namespace CutTheRope.Framework.Visual
{
    internal sealed class HorizontallyTiledImage : Image
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
            float w = texture.quadRects[tiles[0]].w;
            float w2 = texture.quadRects[tiles[2]].w;
            float num = width - (w + w2);
            if (num >= 0f)
            {
                GLDrawer.DrawImageQuad(texture, tiles[0], drawX, drawY + offsets[0]);
                GLDrawer.DrawImageTiledCool(texture, tiles[1], drawX + w, drawY + offsets[1], num, texture.quadRects[tiles[1]].h);
                GLDrawer.DrawImageQuad(texture, tiles[2], drawX + w + num, drawY + offsets[2]);
            }
            else
            {
                CTRRectangle r = texture.quadRects[tiles[0]];
                CTRRectangle r2 = texture.quadRects[tiles[2]];
                r.w = Math.Min(r.w, width / 2f);
                r2.w = Math.Min(r2.w, width - r.w);
                r2.x += texture.quadRects[tiles[2]].w - r2.w;
                GLDrawer.DrawImagePart(texture, r, drawX, drawY + offsets[0]);
                GLDrawer.DrawImagePart(texture, r2, drawX + r.w, drawY + offsets[2]);
            }
            PostDraw();
        }

        public void SetTileHorizontallyLeftCenterRight(int l, int c, int r)
        {
            tiles[0] = l;
            tiles[1] = c;
            tiles[2] = r;
            float h = texture.quadRects[tiles[0]].h;
            float h2 = texture.quadRects[tiles[1]].h;
            float h3 = texture.quadRects[tiles[2]].h;
            height = h >= h2 && h >= h3 ? (int)h : h2 >= h && h2 >= h3 ? (int)h2 : (int)h3;
            offsets[0] = (height - h) / 2f;
            offsets[1] = (height - h2) / 2f;
            offsets[2] = (height - h3) / 2f;
        }

        public static HorizontallyTiledImage HorizontallyTiledImage_create(CTRTexture2D t)
        {
            return (HorizontallyTiledImage)new HorizontallyTiledImage().InitWithTexture(t);
        }

        public static HorizontallyTiledImage HorizontallyTiledImage_createWithResID(int r)
        {
            return HorizontallyTiledImage_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
        }

        /// <summary>
        /// Creates a tiled image from the specified texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        public static HorizontallyTiledImage HorizontallyTiledImage_createWithResID(string resourceName)
        {
            return HorizontallyTiledImage_create(Application.GetTexture(resourceName));
        }

        public static HorizontallyTiledImage HorizontallyTiledImage_createWithResIDQuad(int r, int q)
        {
            HorizontallyTiledImage horizontallyTiledImage = HorizontallyTiledImage_create(Application.GetTexture(ResourceNameTranslator.TranslateLegacyId(r)));
            horizontallyTiledImage.SetDrawQuad(q);
            return horizontallyTiledImage;
        }

        public int[] tiles = new int[3];

        public float[] offsets = new float[3];

        public int align;
    }
}
