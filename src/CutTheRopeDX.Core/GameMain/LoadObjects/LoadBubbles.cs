using System.Xml.Linq;

using CutTheRopeDX.Framework.Visual;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a bubble object from XML node data
        /// </summary>
        /// <param name="xmlNode">The XML node describing the bubble.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadBubble(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            int q2 = RND_RANGE(1, 3);
            Bubble bubble = Bubble.Bubble_createWithResIDQuad(Resources.Img.ObjBubble, q2);
            bubble.DoRestoreCutTransparency();
            bubble.bb = GetBubbleBoundingBox();
            bubble.initial_x = bubble.x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            bubble.initial_y = bubble.y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            bubble.initial_rotation = 0f;
            bubble.initial_rotatedCircle = null;
            bubble.anchor = 18;
            bubble.popped = false;
            Image image = Image.Image_createWithResIDQuad(Resources.Img.ObjBubble, 0);
            image.DoRestoreCutTransparency();
            image.parentAnchor = image.anchor = 18;
            _ = bubble.AddChild(image);
            bubbles.Add(bubble);
        }
    }
}
