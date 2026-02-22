using System;
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
            float x = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            float y = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            bool isCandyCaptured = "true".Equals(xmlNode.AttributeAsNSString("candyCaptured"), StringComparison.OrdinalIgnoreCase);

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
