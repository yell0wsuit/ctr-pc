using CutTheRope.ios;

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
        private void LoadRotatedCircle(XMLNode xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float num9 = (xmlNode["x"].IntValue() * scale) + offsetX + mapOffsetX;
            float num10 = (xmlNode["y"].IntValue() * scale) + offsetY + mapOffsetY;
            float num11 = xmlNode["size"].IntValue();
            float d = xmlNode["handleAngle"].IntValue();
            bool hasOneHandle = xmlNode["oneHandle"].BoolValue();
            RotatedCircle rotatedCircle = (RotatedCircle)new RotatedCircle().Init();
            rotatedCircle.anchor = 18;
            rotatedCircle.x = num9;
            rotatedCircle.y = num10;
            rotatedCircle.rotation = d;
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
