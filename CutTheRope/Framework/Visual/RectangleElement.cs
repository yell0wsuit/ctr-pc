using CutTheRope.Desktop;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Visual
{
    internal class RectangleElement : BaseElement
    {
        public RectangleElement()
        {
            solid = true;
        }

        public override void Draw()
        {
            base.PreDraw();
            OpenGLRenderer.GlDisable(OpenGLRenderer.GL_TEXTURE_2D);
            if (solid)
            {
                GLDrawer.DrawSolidRectWOBorder(drawX, drawY, width, height, color);
            }
            else
            {
                GLDrawer.DrawRect(drawX, drawY, width, height, color);
            }
            OpenGLRenderer.GlEnable(OpenGLRenderer.GL_TEXTURE_2D);
            OpenGLRenderer.GlColor4f(Color.White);
            base.PostDraw();
        }

        public bool solid;
    }
}
