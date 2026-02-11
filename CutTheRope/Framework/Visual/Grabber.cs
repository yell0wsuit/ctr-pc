using CutTheRope.Desktop;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Visual
{
    internal sealed class Grabber : FrameworkTypes
    {
        public static CTRTexture2D Grab()
        {
            return new CTRTexture2D().InitFromPixels((int)SCREEN_WIDTH, (int)SCREEN_HEIGHT);
        }

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
