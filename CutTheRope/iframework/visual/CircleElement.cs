using CutTheRope.desktop;
using CutTheRope.ios;
using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.visual
{
    internal sealed class CircleElement : BaseElement
    {
        public override NSObject Init()
        {
            if (base.Init() != null)
            {
                vertextCount = 32;
                solid = true;
            }
            return this;
        }

        public override void Draw()
        {
            base.PreDraw();
            OpenGL.GlDisable(0);
            _ = MIN(width, height);
            OpenGL.GlEnable(0);
            OpenGL.GlColor4f(Color.White);
            base.PostDraw();
        }

        public bool solid;

        public int vertextCount;
    }
}
