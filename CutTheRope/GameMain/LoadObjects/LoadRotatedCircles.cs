using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading rotated circle objects from XML level data
    /// Rotating circles are interactive puzzle elements the player can rotate
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a rotated circle object from XML node data
        /// </summary>
        private void LoadRotatedCircle(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float centerX = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float centerY = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
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
