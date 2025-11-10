using CutTheRope.desktop;
using CutTheRope.ios;
using Microsoft.Xna.Framework;

namespace CutTheRope.iframework.visual
{
    internal class CircleElement : BaseElement
    {
        public override NSObject init()
        {
            if (base.init() != null)
            {
                vertextCount = 32;
                solid = true;
            }
            return this;
        }

        public override void draw()
        {
            base.preDraw();
            OpenGL.glDisable(0);
            MIN(width, height);
            bool flag = solid;
            OpenGL.glEnable(0);
            OpenGL.glColor4f(Color.White);
            base.postDraw();
        }

        public bool solid;

        public int vertextCount;
    }
}
