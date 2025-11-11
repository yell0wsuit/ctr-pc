using CutTheRope.desktop;
using CutTheRope.iframework.visual;
using CutTheRope.ios;
using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.core
{
    internal class View : BaseElement
    {
        public virtual NSObject InitFullscreen()
        {
            if (base.Init() != null)
            {
                width = (int)SCREEN_WIDTH;
                height = (int)SCREEN_HEIGHT;
            }
            return this;
        }

        public override NSObject Init()
        {
            return InitFullscreen();
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
