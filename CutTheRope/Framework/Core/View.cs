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
            Renderer.GlColor4f(Color.White);
            Renderer.GlEnable(Renderer.GL_TEXTURE_2D);
            Renderer.GlEnable(Renderer.GL_BLEND);
            Renderer.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            base.PreDraw();
            base.PostDraw();
            Renderer.GlDisable(Renderer.GL_TEXTURE_2D);
            Renderer.GlDisable(Renderer.GL_BLEND);
        }
    }
}
