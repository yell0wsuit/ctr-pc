using CutTheRope.Desktop;
using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Image subclass used by the Flash XML animation system.
    /// The atlas sprites are at @2x pixel resolution, but Flash XML coordinates
    /// (positions, anchors, rotation centers) are in @1x point space.
    /// This class scales both width/height (for rotation/scale center in PreDraw)
    /// and the draw size (in DrawQuad) to @1x, keeping everything in a consistent
    /// coordinate space.
    /// </summary>
    internal sealed class FlashXmlImage : Image
    {
        private readonly float _dimensionScale;

        /// <summary>
        /// Multiplier applied to the update delta before advancing timelines.
        /// </summary>
        internal float PlaybackRate { get; set; } = 1f;

        private FlashXmlImage(float dimensionScale)
        {
            _dimensionScale = dimensionScale;
        }

        public override void Update(float delta)
        {
            base.Update(delta * PlaybackRate);
        }

        public override void SetDrawQuad(int n)
        {
            base.SetDrawQuad(n);
            width = (int)(texture.quadRects[n].w * _dimensionScale);
            height = (int)(texture.quadRects[n].h * _dimensionScale);
        }

        public override void DrawQuad(int n)
        {
            float w = texture.quadRects[n].w * _dimensionScale;
            float h = texture.quadRects[n].h * _dimensionScale;
            float x = drawX;
            float y = drawY;
            if (restoreCutTransparency)
            {
                x += texture.quadOffsets[n].X;
                y += texture.quadOffsets[n].Y;
            }
            Quad2D quad = texture.quads[n];
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.BindTexture(texture.Name());
            VertexPositionNormalTexture[] vertices = QuadVertexCache.GetTexturedQuad(
                x, y, w, h,
                quad.tlX, quad.tlY, quad.brX, quad.brY);
            Renderer.DrawTriangleStrip(vertices);
        }

        public static FlashXmlImage CreateWithResID(string resourceName, float dimensionScale)
        {
            FlashXmlImage image = new(dimensionScale);
            _ = image.InitWithTexture(Application.GetTexture(resourceName));
            return image;
        }
    }
}
