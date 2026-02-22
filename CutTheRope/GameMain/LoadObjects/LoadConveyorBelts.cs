using System.Xml.Linq;
using System.Globalization;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadConveyorBelt(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float length = (string.IsNullOrEmpty(xmlNode.Attribute("length")?.Value) ? 0f : float.Parse(xmlNode.Attribute("length")?.Value, CultureInfo.InvariantCulture)) * scale;
            float height = (string.IsNullOrEmpty(xmlNode.Attribute("width")?.Value) ? 0f : float.Parse(xmlNode.Attribute("width")?.Value, CultureInfo.InvariantCulture)) * scale;
            float rotation = string.IsNullOrEmpty(xmlNode.Attribute("angle")?.Value) ? 0f : float.Parse(xmlNode.Attribute("angle")?.Value, CultureInfo.InvariantCulture);
            float velocity = string.IsNullOrEmpty(xmlNode.Attribute("velocity")?.Value) ? 0f : float.Parse(xmlNode.Attribute("velocity")?.Value, CultureInfo.InvariantCulture);
            string direction = xmlNode.Attribute("direction")?.Value ?? string.Empty;
            string type = xmlNode.Attribute("type")?.Value ?? string.Empty;

            float adjustedVelocity = velocity * 0.4f * (direction == "forward" ? 1f : -1f);
            bool isManual = type == "manual";

            ConveyorBelt belt = ConveyorBelt.Create(conveyors.Count(), x, y, length, height, rotation, isManual, adjustedVelocity);
            conveyors.Push(belt);
        }
    }
}
