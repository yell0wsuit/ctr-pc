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
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            if (solid)
            {
                DrawHelper.DrawSolidRectWOBorder(drawX, drawY, width, height, color);
            }
            else
            {
                DrawHelper.DrawRect(drawX, drawY, width, height, color);
            }
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.SetColor(Color.White);
            base.PostDraw();
        }

        public bool solid;
    }
}
