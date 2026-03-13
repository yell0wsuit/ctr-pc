using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading bamboo tube objects from XML level data.
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadBambooTube(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float angle = ParseFloatOrZero(xmlNode.Attribute("angle")?.Value);
            BambooTube bambooTube = new BambooTube().InitWithPositionAngle(Vect(x, y), angle, scale);
            bambooTubes.Add(bambooTube);
        }
    }
}
