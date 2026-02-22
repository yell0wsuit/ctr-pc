using System;
using System.Xml.Linq;

using CutTheRope.Helpers;

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
            float centerX = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            float centerY = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            float circleSize = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("size")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("size"));
            float d = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("handleAngle")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("handleAngle"));
            bool hasOneHandle = "true".Equals(xmlNode.AttributeAsNSString("oneHandle"), StringComparison.OrdinalIgnoreCase);
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
