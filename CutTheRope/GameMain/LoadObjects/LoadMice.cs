using System.Xml.Linq;
using System.Globalization;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadMouse(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty, CultureInfo.InvariantCulture)) * scale) + offsetX + mapOffsetX;
            float py = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty, CultureInfo.InvariantCulture)) * scale) + offsetY + mapOffsetY;
            float angle = string.IsNullOrEmpty(xmlNode.Attribute("angle")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("angle")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
            float radius = string.IsNullOrEmpty(xmlNode.Attribute("radius")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("radius")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
            radius = radius != 0f ? radius * scale : 80f * scale;
            float activeTime = string.IsNullOrEmpty(xmlNode.Attribute("activeTime")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("activeTime")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
            if (activeTime == 0f)
            {
                activeTime = 3f;
            }
            int index = string.IsNullOrEmpty(xmlNode.Attribute("index")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("index")?.Value ?? string.Empty);
            if (index == 0)
            {
                index = mice.Count + 1;
            }

            miceManager ??= new MiceObject(this);

            Mouse mouse = new(miceManager);
            mouse.Initialize(px, py, angle, radius, activeTime);
            mice.Add(mouse);
            miceManager.RegisterMouse(mouse, index);
        }
    }
}
