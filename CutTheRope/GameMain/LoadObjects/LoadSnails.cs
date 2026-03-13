using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadSnail(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;

            Snail snail = Snail.Snail_createWithResIDQuad(Resources.Img.ObjSnail, 8);
            snail.anchor = 18;
            snail.x = x;
            snail.y = y;
            snailobjects.Add(snail);
        }
    }
}
