using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.GameMain
{
    internal static class FlashXmlScale
    {
        internal const float IosRetinaAtlasScale = 2f;
        internal const float PcOutputScale = 0.78f;
        internal const float AtlasToFlashPointScale = IosRetinaAtlasScale * PcOutputScale;

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
        public override void SetDrawQuad(int n)
        {
            base.SetDrawQuad(n);
            if (!restoreCutTransparency)
            {
                width = NormalizeAtlasDimension(texture.quadRects[n].w);
                height = NormalizeAtlasDimension(texture.quadRects[n].h);
            }
        }

        public override void DoRestoreCutTransparency()
        {
            if (texture.preCutSize.X != vectUndefined.X)
            {
                restoreCutTransparency = true;
                width = NormalizeAtlasDimension(texture.preCutSize.X);
                height = NormalizeAtlasDimension(texture.preCutSize.Y);
            }
        }

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

        public static FlashXmlImage CreateWithResID(string resourceName)
        {
            FlashXmlImage image = new();
            _ = image.InitWithTexture(Application.GetTexture(resourceName));
            return image;
        }

        internal static int NormalizeAtlasDimension(float rawValue)
        {
            return (int)FlashXmlScale.NormalizeAtlasValue(rawValue);
        }
    }
}
