using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a conveyor belt object from XML node data.
        /// </summary>
        /// <param name="xmlNode">The XML node describing the conveyor belt.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadConveyorBelt(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float length = ParseFloatOrZero(xmlNode.Attribute("length")?.Value) * scale;
            float height = ParseFloatOrZero(xmlNode.Attribute("width")?.Value) * scale;
            float rotation = ParseFloatOrZero(xmlNode.Attribute("angle")?.Value);
            float velocity = ParseFloatOrZero(xmlNode.Attribute("velocity")?.Value);
            string direction = xmlNode.Attribute("direction")?.Value ?? string.Empty;
            string type = xmlNode.Attribute("type")?.Value ?? string.Empty;

            float adjustedVelocity = scale * velocity * (direction == "forward" ? -1f : 1f);
            bool isManual = type == "manual";

            ConveyorBelt belt = ConveyorBelt.Create(conveyors.Count(), x, y, length, height, rotation, isManual, adjustedVelocity);
            conveyors.Push(belt);
        }
    }
}
