using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Platform;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain.Tutorials
{
    /// <summary>
    /// Hands out recolored copies of tutorial sign frames, one per color a level authors.
    /// </summary>
    /// <remarks>
    /// The ink quads of the sign atlas are pure black, so the vertex-color tint the renderer
    /// applies leaves them black whatever color is asked for. The color goes into a copy of the
    /// frame's pixels instead, built once here and drawn through the ordinary quad path.
    /// </remarks>
    internal sealed class TutorialSignTints : IDisposable
    {
        /// <summary>Gets the frame of <paramref name="atlas"/> recolored to <paramref name="color"/>.</summary>
        /// <param name="atlas">Loaded sign atlas the frame is copied from.</param>
        /// <param name="quad">Zero-based tutorial-sign quad.</param>
        /// <param name="color">Color the frame's ink wears.</param>
        /// <returns>A single-quad texture holding the recolored frame.</returns>
        internal CTRTexture2D Tinted(CTRTexture2D atlas, int quad, RGBAColor color)
        {
            TintKey key = new(quad, color.RedColor, color.GreenColor, color.BlueColor);
            if (tinted.TryGetValue(key, out CTRTexture2D cached))
            {
                return cached;
            }

            CTRRectangle frame = atlas.quadRects[quad];
            int width = (int)frame.w;
            int height = (int)frame.h;
            ITextureHandle handle = AssetPlatform.Current.TintedRegion(
                atlas.textureHandle_,
                (int)frame.x,
                (int)frame.y,
                width,
                height,
                color);

            CTRTexture2D texture = new CTRTexture2D().InitWithHandle(handle, width, height);
            texture.SetQuadsCapacity(1);
            texture.SetQuadAt(new CTRRectangle(0f, 0f, width, height), 0);
            if (atlas.quadOffsets is not null)
            {
                // The copy stands alone, but it has to draw where the frame did inside the atlas.
                texture.quadOffsets[0] = atlas.quadOffsets[quad];
            }

            tinted.Add(key, texture);
            return texture;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (CTRTexture2D texture in tinted.Values)
            {
                // Unregistered by hand: these are built per level rather than cached by resource
                // name, so nothing else would ever take them back out of the global texture list.
                texture.Unreg();
                texture.Dispose();
            }

            tinted.Clear();
        }

        private readonly Dictionary<TintKey, CTRTexture2D> tinted = [];

        private readonly record struct TintKey(int Quad, float Red, float Green, float Blue);
    }
}
