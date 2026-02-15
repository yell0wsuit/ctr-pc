using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadSnail(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            float y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;

            Snail snail = Snail.Snail_createWithResIDQuad(Resources.Img.ObjSnail, 8);
            snail.anchor = 18;
            snail.x = x;
            snail.y = y;
            _ = snailobjects.AddObject(snail);
        }
    }
}
