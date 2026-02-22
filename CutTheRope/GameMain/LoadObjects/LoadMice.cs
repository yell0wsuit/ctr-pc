using System.Xml.Linq;

using CutTheRope.Helpers;
using System.Globalization;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadMouse(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("x"), CultureInfo.InvariantCulture)) * scale) + offsetX + mapOffsetX;
            float py = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("y"), CultureInfo.InvariantCulture)) * scale) + offsetY + mapOffsetY;
            float angle = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("angle")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("angle"), CultureInfo.InvariantCulture);
            float radius = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("radius")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("radius"), CultureInfo.InvariantCulture);
            radius = radius != 0f ? radius * scale : 80f * scale;
            float activeTime = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("activeTime")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("activeTime"), CultureInfo.InvariantCulture);
            if (activeTime == 0f)
            {
                activeTime = 3f;
            }
            int index = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("index")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("index"));
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
