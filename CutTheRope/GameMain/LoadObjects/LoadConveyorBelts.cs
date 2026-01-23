using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadConveyorBelt(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            float y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            float length = xmlNode.AttributeAsNSString("length").FloatValue() * scale;
            float height = xmlNode.AttributeAsNSString("width").FloatValue() * scale;
            float rotation = xmlNode.AttributeAsNSString("angle").FloatValue();
            float velocity = xmlNode.AttributeAsNSString("velocity").FloatValue();
            string direction = xmlNode.AttributeAsNSString("direction");
            string type = xmlNode.AttributeAsNSString("type");

            float adjustedVelocity = velocity * 0.4f * (direction.IsEqualToString("forward") ? 1f : -1f);
            bool isManual = type.IsEqualToString("manual");

            ConveyorBelt belt = ConveyorBelt.Create(conveyors.Count(), x, y, length, height, rotation, isManual, adjustedVelocity);
            conveyors.Push(belt);
        }
    }
}
