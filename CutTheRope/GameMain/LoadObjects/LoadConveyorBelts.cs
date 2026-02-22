using System.Xml.Linq;

using CutTheRope.Helpers;
using System.Globalization;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadConveyorBelt(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            float y = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            float length = (string.IsNullOrEmpty(xmlNode.AttributeAsNSString("length")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("length"), CultureInfo.InvariantCulture)) * scale;
            float height = (string.IsNullOrEmpty(xmlNode.AttributeAsNSString("width")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("width"), CultureInfo.InvariantCulture)) * scale;
            float rotation = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("angle")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("angle"), CultureInfo.InvariantCulture);
            float velocity = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("velocity")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("velocity"), CultureInfo.InvariantCulture);
            string direction = xmlNode.AttributeAsNSString("direction");
            string type = xmlNode.AttributeAsNSString("type");

            float adjustedVelocity = velocity * 0.4f * (direction == "forward" ? 1f : -1f);
            bool isManual = type == "manual";

            ConveyorBelt belt = ConveyorBelt.Create(conveyors.Count(), x, y, length, height, rotation, isManual, adjustedVelocity);
            conveyors.Push(belt);
        }
    }
}
