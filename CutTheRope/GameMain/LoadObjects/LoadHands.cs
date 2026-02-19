using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading mechanical hand objects from XML level data.
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadHand(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            int segmentCount = xmlNode.AttributeAsNSString("segmentsCount").IntValue();

            MechanicalHand hand = new()
            {
                x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX,
                y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY
            };

            CalculateTopLeft(hand);

            for (int i = 1; i <= segmentCount; i++)
            {
                float angle = xmlNode.AttributeAsNSString($"segment{i}Angle").FloatValue();
                if (angle < 0f)
                {
                    angle += 360f;
                }

                float length = xmlNode.AttributeAsNSString($"segment{i}Length").FloatValue() * scale;
                bool rotatable = xmlNode.AttributeAsNSString($"segment{i}Rotatable").BoolValue();
                hand.AddSegmentWithLengthAngleRotatable(length, angle, rotatable);
            }

            CalculateTopLeft(hand.TheClaw());
            hand.Update(0f);
            _ = hands.AddObject(hand);
        }
    }
}
