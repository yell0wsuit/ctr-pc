using System.Xml.Linq;

using CutTheRope.Framework;
using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Load lantern objects from XML.
    /// </summary>
    internal sealed partial class GameScene
    {
        private void LoadLantern(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            float y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            bool isCandyCaptured = xmlNode.AttributeAsNSString("candyCaptured").BoolValue();

            Lantern lantern = new Lantern().InitWithPosition(Vect(x, y));
            lantern.ParseMover(xmlNode);
            if (isCandyCaptured)
            {
                isCandyInLantern = true;
                lantern.CaptureCandy(star);
                candy.x = star.pos.x;
                candy.y = star.pos.y;
                candy.color = RGBAColor.transparentRGBA;
            }
        }
    }
}
