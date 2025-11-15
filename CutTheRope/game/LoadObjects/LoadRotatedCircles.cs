using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.game
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
            float num9 = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            float num10 = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            float num11 = xmlNode.AttributeAsNSString("size").IntValue();
            float d = xmlNode.AttributeAsNSString("handleAngle").IntValue();
            bool hasOneHandle = xmlNode.AttributeAsNSString("oneHandle").BoolValue();
            RotatedCircle rotatedCircle = new()
            {
                anchor = 18,
                x = num9,
                y = num10,
                rotation = d
            };
            rotatedCircle.inithanlde1 = rotatedCircle.handle1 = Vect(rotatedCircle.x - (num11 * scale), rotatedCircle.y);
            rotatedCircle.inithanlde2 = rotatedCircle.handle2 = Vect(rotatedCircle.x + (num11 * scale), rotatedCircle.y);
            rotatedCircle.handle1 = VectRotateAround(rotatedCircle.handle1, (double)DEGREES_TO_RADIANS(d), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.handle2 = VectRotateAround(rotatedCircle.handle2, (double)DEGREES_TO_RADIANS(d), rotatedCircle.x, rotatedCircle.y);
            rotatedCircle.SetSize(num11);
            rotatedCircle.SetHasOneHandle(hasOneHandle);
            _ = rotatedCircles.AddObject(rotatedCircle);
        }
    }
}
