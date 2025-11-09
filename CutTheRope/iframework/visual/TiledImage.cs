using CutTheRope.iframework.core;
using System;

namespace CutTheRope.iframework.visual
{
    internal class TiledImage : Image
    {
        public virtual void setTile(int t)
        {
            this.q = t;
        }

        public override void draw()
        {
            this.preDraw();
            GLDrawer.drawImageTiled(this.texture, this.q, this.drawX, this.drawY, (float)this.width, (float)this.height);
            this.postDraw();
        }

        private static TiledImage TiledImage_create(CTRTexture2D t)
        {
            return (TiledImage)new TiledImage().initWithTexture(t);
        }

        public static TiledImage TiledImage_createWithResID(int r)
        {
            return TiledImage.TiledImage_create(Application.getTexture(r));
        }

        private static TiledImage TiledImage_createWithResIDQuad(int r, int q)
        {
            TiledImage tiledImage = TiledImage.TiledImage_createWithResID(r);
            tiledImage.setDrawQuad(q);
            return tiledImage;
        }

        private int q;
    }
}
