using CutTheRope.desktop;

using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.visual
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
            OpenGL.GlDisable(0);
            if (solid)
            {
                GLDrawer.DrawSolidRectWOBorder(drawX, drawY, width, height, color);
            }
            else
            {
                GLDrawer.DrawRect(drawX, drawY, width, height, color);
            }
            OpenGL.GlEnable(0);
            OpenGL.GlColor4f(Color.White);
            base.PostDraw();
        }

        public bool solid;
    }
}
