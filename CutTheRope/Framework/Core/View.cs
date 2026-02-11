using CutTheRope.Desktop;
using CutTheRope.Framework.Visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.Framework.Core
{
    internal class View : BaseElement
    {
        public View()
        {
            width = (int)SCREEN_WIDTH;
            height = (int)SCREEN_HEIGHT;
        }

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
