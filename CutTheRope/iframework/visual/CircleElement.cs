using CutTheRope.desktop;

using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.visual
{
    internal sealed class CircleElement : BaseElement
    {
        public CircleElement()
        {
            vertextCount = 32;
            solid = true;
        }

        public override void Draw()
        {
            PreDraw();
            OpenGL.GlDisable(0);
            _ = MIN(width, height);
            OpenGL.GlEnable(0);
            OpenGL.GlColor4f(Color.White);
            PostDraw();
        }

        public bool solid;

        public int vertextCount;
    }
}
