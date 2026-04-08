using System;

using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// An <see cref="Image"/> that draws three quads (left, center, right) with the center tiled horizontally to fill the width.
    /// </summary>
    internal sealed class HorizontallyTiledImage : Image
    {
        /// <inheritdoc />
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

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            float w = texture.quadRects[tiles[0]].w;
            float w2 = texture.quadRects[tiles[2]].w;
            float middleWidth = width - (w + w2);
            if (middleWidth >= 0f)
            {
                DrawHelper.DrawImageQuad(texture, tiles[0], drawX, drawY + offsets[0]);
                DrawHelper.DrawImageTiledCool(texture, tiles[1], drawX + w, drawY + offsets[1], middleWidth, texture.quadRects[tiles[1]].h);
                DrawHelper.DrawImageQuad(texture, tiles[2], drawX + w + middleWidth, drawY + offsets[2]);
            }
            else
            {
                CTRRectangle r = texture.quadRects[tiles[0]];
                CTRRectangle r2 = texture.quadRects[tiles[2]];
                r.w = MathF.Min(r.w, width / 2f);
                r2.w = MathF.Min(r2.w, width - r.w);
                r2.x += texture.quadRects[tiles[2]].w - r2.w;
                DrawHelper.DrawImagePart(texture, r, drawX, drawY + offsets[0]);
                DrawHelper.DrawImagePart(texture, r2, drawX + r.w, drawY + offsets[2]);
            }
            PostDraw();
        }

        /// <summary>
        /// Sets the left, center, and right tile quad indices and computes vertical offsets.
        /// </summary>
        /// <param name="l">Left tile quad index.</param>
        /// <param name="c">Center tile quad index (tiled horizontally).</param>
        /// <param name="r">Right tile quad index.</param>
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

        /// <summary>
        /// Creates a horizontally tiled image from the specified texture.
        /// </summary>
        /// <param name="t">Texture to use.</param>
        /// <returns>A new horizontally tiled image instance.</returns>
        public static HorizontallyTiledImage HorizontallyTiledImage_create(CTRTexture2D t)
        {
            return (HorizontallyTiledImage)new HorizontallyTiledImage().InitWithTexture(t);
        }

        /// <summary>
        /// Creates a tiled image from the specified texture resource name.
        /// </summary>
        /// <param name="resourceName">Texture resource name.</param>
        /// <returns>A new horizontally tiled image initialized from the requested resource.</returns>
        public static HorizontallyTiledImage HorizontallyTiledImage_createWithResID(string resourceName)
        {
            return HorizontallyTiledImage_create(Application.GetTexture(resourceName));
        }

        /// <summary>
        /// Quad indices for the left, center, and right tiles.
        /// </summary>
        public int[] tiles = new int[3];

        /// <summary>
        /// Vertical offsets for each tile to center them within the element height.
        /// </summary>
        public float[] offsets = new float[3];

        /// <summary>
        /// Alignment flag for the tiled image.
        /// </summary>
        public int align;
    }
}
