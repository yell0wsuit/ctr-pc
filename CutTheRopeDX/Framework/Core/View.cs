using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRopeDX.Framework.Core
{
    /// <summary>
    /// Full-screen root view used by controllers to host interactive elements.
    /// </summary>
    internal class View : BaseElement
    {
        /// <summary>
        /// Initializes a view sized to the current logical screen dimensions.
        /// </summary>
        public View()
        {
            width = (int)SCREEN_WIDTH;
            height = (int)SCREEN_HEIGHT;
        }

        /// <inheritdoc />
        public override void Draw()
        {
            Renderer.SetColor(Color.White);
            Renderer.Enable(Renderer.GL_TEXTURE_2D);
            Renderer.Enable(Renderer.GL_BLEND);
            Renderer.SetBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            base.PreDraw();
            base.PostDraw();
            Renderer.Disable(Renderer.GL_TEXTURE_2D);
            Renderer.Disable(Renderer.GL_BLEND);
        }
    }
}
