using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

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
            float px2 = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float py2 = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            int w2 = ParseIntOrZero(xmlNode.Attribute("size")?.Value);
            float an2 = ParseIntOrZero(xmlNode.Attribute("angle")?.Value);
            Bouncer bouncer = new Bouncer().InitWithPosXYWidthAndAngle(px2, py2, w2, an2);
            bouncer.ParseMover(xmlNode);
            bouncers.Add(bouncer);
        }
    }
}
