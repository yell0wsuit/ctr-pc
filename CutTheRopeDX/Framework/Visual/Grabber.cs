using CutTheRopeDX.Desktop;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// Captures the current screen into a texture and can redraw it later.
    /// </summary>
    internal sealed class Grabber : FrameworkTypes
    {
        /// <summary>
        /// Captures the current screen contents into a new texture.
        /// </summary>
        /// <returns>A texture containing the captured frame.</returns>
        public static CTRTexture2D Grab()
        {
            return new CTRTexture2D().InitFromPixels((int)SCREEN_WIDTH, (int)SCREEN_HEIGHT);
        }

        /// <summary>
        /// Draws a previously grabbed texture at the specified position.
        /// </summary>
        /// <param name="t">Grabbed texture to draw.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        public static void DrawGrabbedImage(CTRTexture2D t, int x, int y)
        {
            if (t != null)
            {
                Renderer.Enable(Renderer.GL_TEXTURE_2D);
                Renderer.BindTexture(t.Name());
                VertexPositionNormalTexture[] vertices = QuadVertexCache.GetTexturedQuad(
                    x, y, t._realWidth, t._realHeight,
                    0f, 0f, t._maxS, t._maxT);
                Renderer.DrawTriangleStrip(vertices);
            }
        }
    }
}
