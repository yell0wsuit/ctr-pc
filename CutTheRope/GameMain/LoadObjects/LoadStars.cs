using System.Xml.Linq;
using System.Globalization;

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
            star.x = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty)) * scale) + offsetX + mapOffsetX;
            star.y = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty)) * scale) + offsetY + mapOffsetY;
            star.timeout = string.IsNullOrEmpty(xmlNode.Attribute("timeout")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("timeout")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
            star.CreateAnimations();
            star.bb = MakeRectangle(70f, 64f, 82f, 82f);
            star.ParseMover(xmlNode);
            star.Update(0f);
            stars.Add(star);
        }
    }
}
