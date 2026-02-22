using System.Xml.Linq;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadSnail(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty)) * scale) + offsetX + mapOffsetX;
            float y = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty)) * scale) + offsetY + mapOffsetY;

            Snail snail = Snail.Snail_createWithResIDQuad(Resources.Img.ObjSnail, 8);
            snail.anchor = 18;
            snail.x = x;
            snail.y = y;
            snailobjects.Add(snail);
        }
    }
}
