using System.Xml.Linq;

using CutTheRope.Framework.Visual;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading bubble objects from XML level data
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a bubble object from XML node data
        /// </summary>
        private void LoadBubble(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            int q2 = RND_RANGE(1, 3);
            Bubble bubble = Bubble.Bubble_createWithResIDQuad(Resources.Img.ObjBubbleAttached, q2);
            bubble.DoRestoreCutTransparency();
            bubble.bb = MakeRectangle(48f, 48f, 152f, 152f);
            bubble.initial_x = bubble.x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            bubble.initial_y = bubble.y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            bubble.initial_rotation = 0f;
            bubble.initial_rotatedCircle = null;
            bubble.anchor = 18;
            bubble.popped = false;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjBubbleAttached, 0);
            image.DoRestoreCutTransparency();
            image.parentAnchor = image.anchor = 18;
            _ = bubble.AddChild(image);
            bubbles.Add(bubble);
        }
    }
}
