using System.Xml.Linq;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a rotated circle (vinyl) object from XML node data
        /// </summary>
        /// <param name="xmlNode">The XML node describing the rotated circle.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadRotatedCircle(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float centerX = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float centerY = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float circleSize = ParseIntOrZero(xmlNode.Attribute("size")?.Value);
            float d = ParseIntOrZero(xmlNode.Attribute("handleAngle")?.Value);
            _ = bool.TryParse(xmlNode.Attribute("oneHandle")?.Value, out bool hasOneHandle);
            RotatedCircle rotatedCircle = new()
            {
                anchor = 18,
                x = centerX,
                y = centerY,
                rotation = d
            };
            rotatedCircle.inithanlde1 = rotatedCircle.handle1 = Vect(rotatedCircle.x - (circleSize * scale), rotatedCircle.y);
            rotatedCircle.inithanlde2 = rotatedCircle.handle2 = Vect(rotatedCircle.x + (circleSize * scale), rotatedCircle.y);
            rotatedCircle.handle1 = VectRotateAround(rotatedCircle.handle1, DEGREES_TO_RADIANS(d), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.handle2 = VectRotateAround(rotatedCircle.handle2, DEGREES_TO_RADIANS(d), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.SetSize(circleSize);
            rotatedCircle.SetHasOneHandle(hasOneHandle);
            rotatedCircles.Add(rotatedCircle);
        }
    }
}
