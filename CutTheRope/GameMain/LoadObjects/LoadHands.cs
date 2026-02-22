using System;
using System.Xml.Linq;

using CutTheRope.Helpers;
using System.Globalization;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading mechanical hand objects from XML level data.
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadHand(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            int segmentCount = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("segmentsCount")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("segmentsCount"));

            MechanicalHand hand = new()
            {
                x = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX,
                y = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY
            };

            CalculateTopLeft(hand);

            for (int i = 1; i <= segmentCount; i++)
            {
                float angle = string.IsNullOrEmpty(xmlNode.AttributeAsNSString($"segment{i}Angle")) ? 0f : float.Parse(xmlNode.AttributeAsNSString($"segment{i}Angle"), CultureInfo.InvariantCulture);
                if (angle < 0f)
                {
                    angle += 360f;
                }

                float length = (string.IsNullOrEmpty(xmlNode.AttributeAsNSString($"segment{i}Length")) ? 0f : float.Parse(xmlNode.AttributeAsNSString($"segment{i}Length"), CultureInfo.InvariantCulture)) * scale;
                bool rotatable = "true".Equals(xmlNode.AttributeAsNSString($"segment{i}Rotatable"), StringComparison.OrdinalIgnoreCase);
                hand.AddSegmentWithLengthAngleRotatable(length, angle, rotatable);
            }

            CalculateTopLeft(hand.TheClaw());
            hand.Update(0f);
            hands.Add(hand);
        }
    }
}
