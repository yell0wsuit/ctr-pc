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
            OpenGL.GlColor4f(Color.White);
            OpenGL.GlEnable(OpenGL.GL_TEXTURE_2D);
            OpenGL.GlEnable(OpenGL.GL_BLEND);
            OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            base.PreDraw();
            base.PostDraw();
            OpenGL.GlDisable(OpenGL.GL_TEXTURE_2D);
            OpenGL.GlDisable(OpenGL.GL_BLEND);
        }
    }
}
