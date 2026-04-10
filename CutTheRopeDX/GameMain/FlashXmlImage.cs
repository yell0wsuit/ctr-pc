using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework;
using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Scale conversion constants and helpers for Flash XML atlas coordinates.
    /// </summary>
    internal static class FlashXmlScale
    {
        /// <summary>Scale factor used by the iOS retina atlas export.</summary>
        internal const float IosRetinaAtlasScale = 2f;

        /// <summary>Scale factor used by the PC output assets.</summary>
        internal const float PcOutputScale = 0.78f;

        /// <summary>Combined conversion factor from atlas pixels to Flash point units.</summary>
        internal const float AtlasToFlashPointScale = IosRetinaAtlasScale * PcOutputScale;

        /// <summary>
        /// Converts an atlas-space value to Flash point space.
        /// </summary>
        /// <param name="rawValue">Atlas-space value to normalize.</param>
        /// <returns>The value in Flash point space, or zero when <paramref name="rawValue" /> is zero.</returns>
        internal static float NormalizeAtlasValue(float rawValue)
        {
            return rawValue == 0f
                ? 0f
                : rawValue / AtlasToFlashPointScale;
        }
    }

    /// <summary>
    /// Image subclass used by the Flash XML animation system.
    /// The source atlas is stored in PC-exported retina space, so quad sizes and
    /// offsets need to be normalized back into Flash XML point space before
    /// BaseElement anchor and rotation math runs.
    /// </summary>
    internal sealed class FlashXmlImage : Image
    {
        /// <inheritdoc />
        public override void SetDrawQuad(int n)
        {
            base.SetDrawQuad(n);
            if (!restoreCutTransparency)
            {
                width = NormalizeAtlasDimension(texture.quadRects[n].w);
                height = NormalizeAtlasDimension(texture.quadRects[n].h);
            }
        }

        /// <inheritdoc />
        public override void DoRestoreCutTransparency()
        {
            if (texture.preCutSize.X != vectUndefined.X)
            {
                restoreCutTransparency = true;
                width = NormalizeAtlasDimension(texture.preCutSize.X);
                height = NormalizeAtlasDimension(texture.preCutSize.Y);
            }
        }

        /// <inheritdoc />
        public override void DrawQuad(int n)
        {
            float w = FlashXmlScale.NormalizeAtlasValue(texture.quadRects[n].w);
            float h = FlashXmlScale.NormalizeAtlasValue(texture.quadRects[n].h);
            float x = drawX;
            float y = drawY;
            if (restoreCutTransparency)
            {
                x += FlashXmlScale.NormalizeAtlasValue(texture.quadOffsets[n].X);
                y += FlashXmlScale.NormalizeAtlasValue(texture.quadOffsets[n].Y);
            }
            Quad2D quad = texture.quads[n];
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(texture.Name());
            VertexPositionNormalTexture[] vertices = QuadVertexCache.GetTexturedQuad(
                x, y, w, h,
                quad.tlX, quad.tlY, quad.brX, quad.brY);
            Renderer.DrawTriangleStrip(vertices);
        }

        /// <summary>
        /// Creates a Flash XML image for the specified texture resource.
        /// </summary>
        /// <param name="resourceName">Texture resource name to load.</param>
        /// <returns>The initialized Flash XML image.</returns>
        public static FlashXmlImage CreateWithResID(string resourceName)
        {
            FlashXmlImage image = new();
            _ = image.InitWithTexture(Application.GetTexture(resourceName));
            return image;
        }

        /// <summary>
        /// Converts an atlas-space dimension to an integer Flash point dimension.
        /// </summary>
        /// <param name="rawValue">Atlas-space dimension to normalize.</param>
        /// <returns>The normalized Flash point dimension.</returns>
        internal static int NormalizeAtlasDimension(float rawValue)
        {
            return (int)FlashXmlScale.NormalizeAtlasValue(rawValue);
        }
    }
}
