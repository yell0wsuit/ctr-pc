using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading bouncer objects from XML level data
    /// Bouncers propel the candy upward or in directions
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a bouncer object from XML node data
        /// </summary>
        private void LoadBouncer(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px2 = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            float py2 = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            int w2 = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("size")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("size"));
            float an2 = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("angle")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("angle"));
            Bouncer bouncer = new Bouncer().InitWithPosXYWidthAndAngle(px2, py2, w2, an2);
            bouncer.ParseMover(xmlNode);
            bouncers.Add(bouncer);
        }
    }
}
