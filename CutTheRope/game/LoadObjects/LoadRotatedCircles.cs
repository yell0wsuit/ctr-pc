using CutTheRope.desktop;
using CutTheRope.iframework;
using CutTheRope.iframework.core;
using CutTheRope.iframework.helpers;
using CutTheRope.iframework.sfe;
using CutTheRope.iframework.visual;
using CutTheRope.ios;

namespace CutTheRope.game
{
    /// <summary>
    /// Handles loading rotated circle objects from XML level data
    /// Rotating circles are interactive puzzle elements the player can rotate
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadRotatedCircle(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (item.Name != "rotatedCircle") return;

            float num9 = (item["x"].IntValue() * scale) + offsetX + mapOffsetX;
            float num10 = (item["y"].IntValue() * scale) + offsetY + mapOffsetY;
            float num11 = item["size"].IntValue();
            float d = item["handleAngle"].IntValue();
            bool hasOneHandle = item["oneHandle"].BoolValue();
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
