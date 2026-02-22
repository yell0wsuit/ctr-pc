using System;
using System.Xml.Linq;

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
            float centerX = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty)) * scale) + offsetX + mapOffsetX;
            float centerY = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty)) * scale) + offsetY + mapOffsetY;
            float circleSize = string.IsNullOrEmpty(xmlNode.Attribute("size")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("size")?.Value ?? string.Empty);
            float d = string.IsNullOrEmpty(xmlNode.Attribute("handleAngle")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("handleAngle")?.Value ?? string.Empty);
            bool hasOneHandle = "true".Equals(xmlNode.Attribute("oneHandle")?.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
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
