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
            int segmentCount = ParseIntOrZero(xmlNode.Attribute("segmentsCount")?.Value);

            MechanicalHand hand = new()
            {
                x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX,
                y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY
            };

            CalculateTopLeft(hand);

            for (int i = 1; i <= segmentCount; i++)
            {
                float angle = string.IsNullOrEmpty(xmlNode.Attribute($"segment{i}Angle")?.Value) ? 0f : float.Parse(xmlNode.Attribute($"segment{i}Angle")?.Value, CultureInfo.InvariantCulture);
                if (angle < 0f)
                {
                    angle += 360f;
                }

                float length = (string.IsNullOrEmpty(xmlNode.Attribute($"segment{i}Length")?.Value) ? 0f : float.Parse(xmlNode.Attribute($"segment{i}Length")?.Value, CultureInfo.InvariantCulture)) * scale;
                _ = bool.TryParse(xmlNode.Attribute($"segment{i}Rotatable")?.Value, out bool rotatable);
                hand.AddSegmentWithLengthAngleRotatable(length, angle, rotatable);
            }

            CalculateTopLeft(hand.TheClaw());
            hand.Update(0f);
            hands.Add(hand);
        }
    }
}
