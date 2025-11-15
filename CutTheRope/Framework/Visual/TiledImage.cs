using CutTheRope.Framework.Core;

namespace CutTheRope.Framework.Visual
{
    internal sealed class TiledImage : Image
    {
        public void SetTile(int t)
        {
            q = t;
        }

        public override void Draw()
        {
            PreDraw();
            GLDrawer.DrawImageTiled(texture, q, drawX, drawY, width, height);
            PostDraw();
        }

        private static TiledImage TiledImage_create(CTRTexture2D t)
        {
            return (TiledImage)new TiledImage().InitWithTexture(t);
        }

        public static TiledImage TiledImage_createWithResID(int r)
        {
            return TiledImage_create(Application.GetTexture(r));
        }

        private int q;
    }
}
