using System;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// An <see cref="Image"/> that draws three quads (top, center, bottom) with the center tiled vertically to fill the height.
    /// </summary>
    internal sealed class VerticallyTiledImage : Image
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
            float h = texture.quadRects[tiles[0]].h;
            float h2 = texture.quadRects[tiles[2]].h;
            float middleHeight = height - (h + h2);
            if (middleHeight >= 0f)
            {
                DrawHelper.DrawImageQuad(texture, tiles[0], drawX + offsets[0], drawY);
                DrawHelper.DrawImageTiledCool(texture, tiles[1], drawX + offsets[1], drawY + h, width, middleHeight);
                DrawHelper.DrawImageQuad(texture, tiles[2], drawX + offsets[2], drawY + h + middleHeight);
            }
            else
            {
                CTRRectangle r = texture.quadRects[tiles[0]];
                CTRRectangle r2 = texture.quadRects[tiles[2]];
                r.h = MathF.Min(r.h, height / 2f);
                r2.h = MathF.Min(r2.h, height - r.h);
                r2.y += texture.quadRects[tiles[2]].h - r2.h;
                DrawHelper.DrawImagePart(texture, r, drawX + offsets[0], drawY);
                DrawHelper.DrawImagePart(texture, r2, drawX + offsets[2], drawY + r.h);
            }
            PostDraw();
        }

        /// <summary>
        /// Sets the top, center, and bottom tile quad indices and computes horizontal offsets.
        /// </summary>
        /// <param name="t">Top tile quad index.</param>
        /// <param name="c">Center tile quad index (tiled vertically).</param>
        /// <param name="b">Bottom tile quad index.</param>
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

        /// <summary>
        /// Quad indices for the top, center, and bottom tiles.
        /// </summary>
        public int[] tiles = new int[3];

        /// <summary>
        /// Horizontal offsets for each tile to center them within the element width.
        /// </summary>
        public float[] offsets = new float[3];

        /// <summary>
        /// Alignment flag for the tiled image.
        /// </summary>
        public int align;
    }
}
