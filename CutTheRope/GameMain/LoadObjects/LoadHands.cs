using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a mechanical hand object from XML node data.
        /// </summary>
        /// <param name="xmlNode">The XML node describing the mechanical hand.</param>
        /// <param name="scale">The level scale factor applied to object coordinates and segment lengths.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
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
