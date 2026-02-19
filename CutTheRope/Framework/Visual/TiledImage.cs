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
            DrawHelper.DrawImageTiled(texture, q, drawX, drawY, width, height);
            PostDraw();
        }

        private static TiledImage TiledImage_create(CTRTexture2D t)
        {
            return (TiledImage)new TiledImage().InitWithTexture(t);
        }

        public static TiledImage TiledImage_createWithResID(string resourceName)
        {
            return TiledImage_create(Application.GetTexture(resourceName));
        }

        private int q;
    }
}
