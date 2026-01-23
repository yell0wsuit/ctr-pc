using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadMouse(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = (xmlNode.AttributeAsNSString("x").FloatValue() * scale) + offsetX + mapOffsetX;
            float py = (xmlNode.AttributeAsNSString("y").FloatValue() * scale) + offsetY + mapOffsetY;
            float angle = xmlNode.AttributeAsNSString("angle").FloatValue();
            float radius = xmlNode.AttributeAsNSString("radius").FloatValue();
            radius = radius != 0f ? radius * scale : 80f * scale;
            float activeTime = xmlNode.AttributeAsNSString("activeTime").FloatValue();
            if (activeTime == 0f)
            {
                activeTime = 3f;
            }
            int index = xmlNode.AttributeAsNSString("index").IntValue();
            if (index == 0)
            {
                index = mice.Count + 1;
            }

            miceManager ??= new MiceObject(this);

            Mouse mouse = new(miceManager);
            mouse.Initialize(px, py, angle, radius, activeTime);
            _ = mice.AddObject(mouse);
            miceManager.RegisterMouse(mouse, index);
        }
    }
}
