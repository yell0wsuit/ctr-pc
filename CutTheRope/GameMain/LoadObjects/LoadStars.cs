using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// GameScene.LoadStars - Partial class handling loading of star objects from XML
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a star object from XML node data
        /// </summary>
        private void LoadStar(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            Star star = Star.Star_createWithResID(Resources.Img.ObjStarIdle);
            if (nightLevel)
            {
                star.EnableNightMode();
            }
            star.x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            star.y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            star.timeout = ParseFloatOrZero(xmlNode.Attribute("timeout")?.Value);
            star.CreateAnimations();
            star.bb = GetStarBoundingBox();
            star.ParseMover(xmlNode);
            star.Update(0f);
            stars.Add(star);
        }
    }
}
