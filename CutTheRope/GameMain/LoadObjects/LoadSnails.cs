using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadSnail(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            float y = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;

            Snail snail = Snail.Snail_createWithResIDQuad(Resources.Img.ObjSnail, 8);
            snail.anchor = 18;
            snail.x = x;
            snail.y = y;
            snailobjects.Add(snail);
        }
    }
}
