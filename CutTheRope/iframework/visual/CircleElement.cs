using CutTheRope.iframework.helpers;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;

namespace CutTheRope.iframework.visual
{
    internal class CircleElement : BaseElement
    {
        public override NSObject init()
        {
            if (base.init() != null)
            {
                this.vertextCount = 32;
                this.solid = true;
            }
            return this;
        }

        public override void draw()
        {
            base.preDraw();
            OpenGL.glDisable(0);
            CTRMathHelper.MIN(this.width, this.height);
            bool flag = this.solid;
            OpenGL.glEnable(0);
            OpenGL.glColor4f(Color.White);
            base.postDraw();
        }

        public bool solid;

        public int vertextCount;
    }
}
