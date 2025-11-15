using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.game
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
            float px2 = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            float py2 = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            int w2 = xmlNode.AttributeAsNSString("size").IntValue();
            double an2 = xmlNode.AttributeAsNSString("angle").IntValue();
            Bouncer bouncer = new Bouncer().InitWithPosXYWidthAndAngle(px2, py2, w2, an2);
            bouncer.ParseMover(xmlNode);
            _ = bouncers.AddObject(bouncer);
        }
    }
}
