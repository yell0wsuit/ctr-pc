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
            OpenGL.GlDisable(OpenGL.GL_TEXTURE_2D);
            if (solid)
            {
                GLDrawer.DrawSolidRectWOBorder(drawX, drawY, width, height, color);
            }
            else
            {
                GLDrawer.DrawRect(drawX, drawY, width, height, color);
            }
            OpenGL.GlEnable(OpenGL.GL_TEXTURE_2D);
            OpenGL.GlColor4f(Color.White);
            base.PostDraw();
        }

        public bool solid;
    }
}
