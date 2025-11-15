using CutTheRope.desktop;
using CutTheRope.iframework.visual;

using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.core
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
            OpenGL.GlEnable(0);
            OpenGL.GlEnable(1);
            OpenGL.GlBlendFunc(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            base.PreDraw();
            base.PostDraw();
            OpenGL.GlDisable(0);
            OpenGL.GlDisable(1);
        }
    }
}
