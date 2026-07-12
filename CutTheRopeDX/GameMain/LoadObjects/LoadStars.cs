using System.Xml.Linq;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a star object from XML node data
        /// </summary>
        /// <param name="xmlNode">The XML node describing the collectible star.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadStar(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            Star star = Star.Star_createWithResID(Resources.Img.ObjStarIdle);
            if (nightLevel)
            {
                star.EnableNightMode();
            }
            star.x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            star.y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            star.timeout = ParseFloatOrZero(xmlNode.Attribute("timeout")?.Value);
            star.CreateAnimations();
            star.bb = GetStarBoundingBox();
            star.ParseMover(xmlNode);
            star.Update(0f);
            stars.Add(star);
        }
    }
}
