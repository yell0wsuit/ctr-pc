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
    /// Handles loading bouncer objects from XML level data
    /// Bouncers propel the candy upward or in directions
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadBouncer(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (!item.Name.Contains("bouncer")) return;

            float px2 = (item["x"].IntValue() * scale) + offsetX + mapOffsetX;
            float py2 = (item["y"].IntValue() * scale) + offsetY + mapOffsetY;
            int w2 = item["size"].IntValue();
            double an2 = item["angle"].IntValue();
            Bouncer bouncer = (Bouncer)new Bouncer().InitWithPosXYWidthAndAngle(px2, py2, w2, an2);
            bouncer.ParseMover(item);
            _ = bouncers.AddObject(bouncer);
        }
    }
}
