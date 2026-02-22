using System.Xml.Linq;
using System.Globalization;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadConveyorBelt(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty)) * scale) + offsetX + mapOffsetX;
            float y = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty)) * scale) + offsetY + mapOffsetY;
            float length = (string.IsNullOrEmpty(xmlNode.Attribute("length")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("length")?.Value ?? string.Empty, CultureInfo.InvariantCulture)) * scale;
            float height = (string.IsNullOrEmpty(xmlNode.Attribute("width")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("width")?.Value ?? string.Empty, CultureInfo.InvariantCulture)) * scale;
            float rotation = string.IsNullOrEmpty(xmlNode.Attribute("angle")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("angle")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
            float velocity = string.IsNullOrEmpty(xmlNode.Attribute("velocity")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("velocity")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
            string direction = xmlNode.Attribute("direction")?.Value ?? string.Empty;
            string type = xmlNode.Attribute("type")?.Value ?? string.Empty;

            float adjustedVelocity = velocity * 0.4f * (direction == "forward" ? 1f : -1f);
            bool isManual = type == "manual";

            ConveyorBelt belt = ConveyorBelt.Create(conveyors.Count(), x, y, length, height, rotation, isManual, adjustedVelocity);
            conveyors.Push(belt);
        }
    }
}
