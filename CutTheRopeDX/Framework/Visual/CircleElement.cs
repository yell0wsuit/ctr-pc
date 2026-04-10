using CutTheRopeDX.Desktop;

using Microsoft.Xna.Framework;

namespace CutTheRopeDX.Framework.Visual
{
    /// <summary>
    /// A <see cref="BaseElement"/> that draws a circle shape.
    /// </summary>
    internal sealed class CircleElement : BaseElement
    {
        /// <summary>
        /// Initializes a new <see cref="CircleElement"/> with default vertex count and solid fill.
        /// </summary>
        public CircleElement()
        {
            vertextCount = 32;
            solid = true;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            _ = MIN(width, height);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.SetColor(Color.White);
            PostDraw();
        }

        /// <summary>
        /// Whether the circle is drawn filled or as an outline.
        /// </summary>
        public bool solid;

        /// <summary>
        /// Number of vertices used to approximate the circle.
        /// </summary>
        public int vertextCount;
    }
}
