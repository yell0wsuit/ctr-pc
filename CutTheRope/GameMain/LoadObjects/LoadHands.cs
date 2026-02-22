using System;
using System.Xml.Linq;
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
            int segmentCount = string.IsNullOrEmpty(xmlNode.Attribute("segmentsCount")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("segmentsCount")?.Value ?? string.Empty);

            MechanicalHand hand = new()
            {
                x = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty)) * scale) + offsetX + mapOffsetX,
                y = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty)) * scale) + offsetY + mapOffsetY
            };

            CalculateTopLeft(hand);

            for (int i = 1; i <= segmentCount; i++)
            {
                float angle = string.IsNullOrEmpty(xmlNode.Attribute($"segment{i}Angle")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute($"segment{i}Angle")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
                if (angle < 0f)
                {
                    angle += 360f;
                }

                float length = (string.IsNullOrEmpty(xmlNode.Attribute($"segment{i}Length")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute($"segment{i}Length")?.Value ?? string.Empty, CultureInfo.InvariantCulture)) * scale;
                bool rotatable = "true".Equals(xmlNode.Attribute($"segment{i}Rotatable")?.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                hand.AddSegmentWithLengthAngleRotatable(length, angle, rotatable);
            }

            CalculateTopLeft(hand.TheClaw());
            hand.Update(0f);
            hands.Add(hand);
        }
    }
}
