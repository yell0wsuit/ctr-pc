using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
    /// <summary>
    /// Handles loading bubble objects from XML level data
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadBubble(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (item.Name != "bubble") return;

            int q2 = RND_RANGE(1, 3);
            Bubble bubble = Bubble.Bubble_createWithResIDQuad(75, q2);
            bubble.DoRestoreCutTransparency();
            bubble.bb = MakeRectangle(48.0, 48.0, 152.0, 152.0);
            bubble.initial_x = bubble.x = (item["x"].IntValue() * scale) + offsetX + mapOffsetX;
            bubble.initial_y = bubble.y = (item["y"].IntValue() * scale) + offsetY + mapOffsetY;
            bubble.initial_rotation = 0f;
            bubble.initial_rotatedCircle = null;
            bubble.anchor = 18;
            bubble.popped = false;
            Image image = Image.Image_createWithResIDQuad(75, 0);
            image.DoRestoreCutTransparency();
            image.parentAnchor = image.anchor = 18;
            _ = bubble.AddChild(image);
            _ = bubbles.AddObject(bubble);
        }
    }
}
