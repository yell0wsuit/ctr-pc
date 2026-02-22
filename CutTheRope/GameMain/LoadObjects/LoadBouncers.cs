using System.Xml.Linq;

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
            float px2 = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty)) * scale) + offsetX + mapOffsetX;
            float py2 = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty)) * scale) + offsetY + mapOffsetY;
            int w2 = string.IsNullOrEmpty(xmlNode.Attribute("size")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("size")?.Value ?? string.Empty);
            float an2 = string.IsNullOrEmpty(xmlNode.Attribute("angle")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("angle")?.Value ?? string.Empty);
            Bouncer bouncer = new Bouncer().InitWithPosXYWidthAndAngle(px2, py2, w2, an2);
            bouncer.ParseMover(xmlNode);
            bouncers.Add(bouncer);
        }
    }
}
