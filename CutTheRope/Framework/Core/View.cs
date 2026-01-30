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
            OpenGLRenderer.GlColor4f(Color.White);
            OpenGLRenderer.GlEnable(OpenGLRenderer.GL_TEXTURE_2D);
            OpenGLRenderer.GlEnable(OpenGLRenderer.GL_BLEND);
            OpenGLRenderer.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            base.PreDraw();
            base.PostDraw();
            OpenGLRenderer.GlDisable(OpenGLRenderer.GL_TEXTURE_2D);
            OpenGLRenderer.GlDisable(OpenGLRenderer.GL_BLEND);
        }
    }
}
