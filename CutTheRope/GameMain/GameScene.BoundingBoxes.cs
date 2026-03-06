using CutTheRope.Framework;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private static CTRRectangle SelectPhysicsBoundingBox(
            float desktopX,
            float desktopY,
            float desktopWidth,
            float desktopHeight,
            float phoneX,
            float phoneY,
            float phoneWidth,
            float phoneHeight)
        {
            if (!ActivePhysicsConstants.UseMobilePhysicsModel)
            {
                return MakeRectangle(desktopX, desktopY, desktopWidth, desktopHeight);
            }

            float scale = ActivePhysicsConstants.Wp7ToWorldScale;
            return MakeRectangle(phoneX * scale, phoneY * scale, phoneWidth * scale, phoneHeight * scale);
        }

        private static CTRRectangle GetCandyBoundingBox()
        {
            return SelectPhysicsBoundingBox(142f, 157f, 112f, 104f, 46f, 49f, 35f, 35f);
        }

        private static CTRRectangle GetSplitCandyBoundingBox()
        {
            return SelectPhysicsBoundingBox(155f, 176f, 88f, 76f, 52f, 56f, 23f, 24f);
        }

        internal static CTRRectangle GetBubbleBoundingBox()
        {
            return SelectPhysicsBoundingBox(48f, 48f, 152f, 152f, 0f, 0f, 57f, 57f);
        }

        internal static CTRRectangle GetSnailBoundingBox()
        {
            return SelectPhysicsBoundingBox(133f, 171f, 120f, 138f, 43f, 55f, 38f, 44f);
        }

        private static CTRRectangle GetPumpBoundingBox()
        {
            return SelectPhysicsBoundingBox(300f, 300f, 175f, 175f, 94f, 95f, 57f, 57f);
        }

        // private static CTRRectangle GetTargetBoundingBox()
        // {
        //     return SelectPhysicsBoundingBox(264f, 350f, 108f, 2f, 90f, 110f, 25f, 1f);
        // }

        private static CTRRectangle GetStarBoundingBox()
        {
            return SelectPhysicsBoundingBox(70f, 64f, 82f, 82f, 22f, 20f, 30f, 30f);
        }
    }
}
