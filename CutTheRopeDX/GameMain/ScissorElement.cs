using CutTheRopeDX.Desktop;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Element that draws its children through the renderer scissor rectangle.
    /// </summary>
    internal sealed class ScissorElement : BaseElement
    {
        /// <inheritdoc />
        public override void Draw()
        {
            PreDraw();
            Renderer.Enable(Renderer.GL_SCISSOR_TEST);
            Renderer.SetScissor(drawX, drawY, width, height);
            PostDraw();
            Renderer.Disable(Renderer.GL_SCISSOR_TEST);
        }
    }
}
