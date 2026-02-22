using System.Xml.Linq;

using CutTheRope.Framework;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Load lantern objects from XML.
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadLantern(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            _ = bool.TryParse(xmlNode.Attribute("candyCaptured")?.Value, out bool isCandyCaptured);

            Lantern lantern = new Lantern().InitWithPosition(Vect(x, y));
            lantern.ParseMover(xmlNode);
            if (isCandyCaptured)
            {
                isCandyInLantern = true;
                lantern.CaptureCandy(star);
                candy.x = star.pos.X;
                candy.y = star.pos.Y;
                candy.color = RGBAColor.transparentRGBA;
            }
        }
    }
}
