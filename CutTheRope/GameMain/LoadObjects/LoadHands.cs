using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

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
                float angle = ParseFloatOrZero(xmlNode.Attribute($"segment{i}Angle")?.Value);
                if (angle < 0f)
                {
                    angle += 360f;
                }

                float length = ParseFloatOrZero(xmlNode.Attribute($"segment{i}Length")?.Value) * scale;
                _ = bool.TryParse(xmlNode.Attribute($"segment{i}Rotatable")?.Value, out bool rotatable);
                hand.AddSegmentWithLengthAngleRotatable(length, angle, rotatable);
            }

            CalculateTopLeft(hand.TheClaw());
            hand.Update(0f);
            hands.Add(hand);
        }
    }
}
