using CutTheRope.Desktop;
using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    internal sealed class ScissorElement : BaseElement
    {
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
