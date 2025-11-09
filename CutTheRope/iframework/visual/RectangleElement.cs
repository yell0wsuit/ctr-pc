using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework;
using System;

namespace CutTheRope.iframework.visual
{
    internal class RectangleElement : BaseElement
    {
        public override NSObject init()
        {
            if (base.init() != null)
            {
                this.solid = true;
            }
            return this;
        }

        public override void draw()
        {
            base.preDraw();
            OpenGL.glDisable(0);
            if (this.solid)
            {
                GLDrawer.drawSolidRectWOBorder(this.drawX, this.drawY, (float)this.width, (float)this.height, this.color);
            }
            else
            {
                GLDrawer.drawRect(this.drawX, this.drawY, (float)this.width, (float)this.height, this.color);
            }
            OpenGL.glEnable(0);
            OpenGL.glColor4f(Color.White);
            base.postDraw();
        }

        public bool solid;
    }
}
