using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.visual;

namespace CutTheRope.game
{
    internal sealed class Bubble : GameObject
    {
        public static Bubble Bubble_create(CTRTexture2D t)
        {
            return (Bubble)new Bubble().InitWithTexture(t);
        }

        public static Bubble Bubble_createWithResID(int r)
        {
            return Bubble_create(Application.GetTexture(r));
        }

        public static Bubble Bubble_createWithResIDQuad(int r, int q)
        {
            Bubble bubble = Bubble_create(Application.GetTexture(r));
            bubble.SetDrawQuad(q);
            return bubble;
        }

        public override void Draw()
        {
            base.PreDraw();
            if (!withoutShadow)
            {
                if (quadToDraw == -1)
                {
                    GLDrawer.DrawImage(texture, drawX, drawY);
                }
                else
                {
                    DrawQuad(quadToDraw);
                }
            }
            base.PostDraw();
        }

        public bool popped;

        public float initial_rotation;

        public float initial_x;

        public float initial_y;

        public RotatedCircle initial_rotatedCircle;

        public bool withoutShadow;
    }
}
