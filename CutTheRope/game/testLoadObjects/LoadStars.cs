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
    /// GameScene.LoadStars - Partial class handling loading of star objects from XML
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadStar(XMLNode item, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            if (item.Name != "star") return;

            Star star = Star.Star_createWithResID(78);
            star.x = (item["x"].IntValue() * scale) + offsetX + mapOffsetX;
            star.y = (item["y"].IntValue() * scale) + offsetY + mapOffsetY;
            star.timeout = item["timeout"].FloatValue();
            star.CreateAnimations();
            star.bb = MakeRectangle(70.0, 64.0, 82.0, 82.0);
            star.ParseMover(item);
            star.Update(0f);
            _ = stars.AddObject(star);
        }
    }
}
