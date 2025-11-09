using CutTheRope.iframework.core;
using System;

namespace CutTheRope.iframework.visual
{
    internal class TiledImage : Image
    {
        public virtual void setTile(int t)
        {
            q = t;
        }

        public override void draw()
        {
            preDraw();
            GLDrawer.drawImageTiled(texture, q, drawX, drawY, width, height);
            postDraw();
        }

        private static TiledImage TiledImage_create(CTRTexture2D t)
        {
            return (TiledImage)new TiledImage().initWithTexture(t);
        }

        public static TiledImage TiledImage_createWithResID(int r)
        {
            return TiledImage_create(Application.getTexture(r));
        }

        private static TiledImage TiledImage_createWithResIDQuad(int r, int q)
        {
            TiledImage tiledImage = TiledImage_createWithResID(r);
            tiledImage.setDrawQuad(q);
            return tiledImage;
        }

        private int q;
    }
}
