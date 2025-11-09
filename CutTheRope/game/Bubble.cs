using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;
using System;

namespace CutTheRope.game
{
    internal class Bubble : GameObject
    {
        public static Bubble Bubble_create(CTRTexture2D t)
        {
            return (Bubble)new Bubble().initWithTexture(t);
        }

        public static Bubble Bubble_createWithResID(int r)
        {
            return Bubble.Bubble_create(Application.getTexture(r));
        }

        public static Bubble Bubble_createWithResIDQuad(int r, int q)
        {
            Bubble bubble = Bubble.Bubble_create(Application.getTexture(r));
            bubble.setDrawQuad(q);
            return bubble;
        }

        public override void draw()
        {
            base.preDraw();
            if (!this.withoutShadow)
            {
                if (this.quadToDraw == -1)
                {
                    GLDrawer.drawImage(this.texture, this.drawX, this.drawY);
                }
                else
                {
                    this.drawQuad(this.quadToDraw);
                }
            }
            base.postDraw();
        }

        public bool popped;

        public float initial_rotation;

        public float initial_x;

        public float initial_y;

        public RotatedCircle initial_rotatedCircle;

        public bool withoutShadow;
    }
}
