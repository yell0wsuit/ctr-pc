using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.game
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
            Star star = Star.Star_createWithResID(78);
            star.x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            star.y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            star.timeout = xmlNode.AttributeAsNSString("timeout").FloatValue();
            star.CreateAnimations();
            star.bb = MakeRectangle(70.0, 64.0, 82.0, 82.0);
            star.ParseMover(xmlNode);
            star.Update(0f);
            _ = stars.AddObject(star);
        }
    }
}
