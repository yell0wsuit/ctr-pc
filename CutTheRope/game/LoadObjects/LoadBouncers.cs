using CutTheRope.ios;

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
        private void LoadBouncer(XMLNode xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px2 = (xmlNode["x"].IntValue() * scale) + offsetX + mapOffsetX;
            float py2 = (xmlNode["y"].IntValue() * scale) + offsetY + mapOffsetY;
            int w2 = xmlNode["size"].IntValue();
            double an2 = xmlNode["angle"].IntValue();
            Bouncer bouncer = (Bouncer)new Bouncer().InitWithPosXYWidthAndAngle(px2, py2, w2, an2);
            bouncer.ParseMover(xmlNode);
            _ = bouncers.AddObject(bouncer);
        }
    }
}
