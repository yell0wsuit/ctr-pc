using CutTheRope.Desktop;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> that draws a colored rectangle, either filled or outline.
    /// </summary>
    internal class RectangleElement : BaseElement
    {
        /// <summary>
        /// Initializes a new <see cref="RectangleElement"/> with solid fill enabled.
        /// </summary>
        public RectangleElement()
        {
            solid = true;
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Whether the rectangle is drawn filled or as an outline.
        /// </summary>
        public bool solid;
    }
}
