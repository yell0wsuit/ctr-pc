using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a mouse object from XML node data.
        /// </summary>
        /// <param name="xmlNode">The XML node describing the mouse.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadMouse(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = (ParseFloatOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float py = (ParseFloatOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float angle = ParseFloatOrZero(xmlNode.Attribute("angle")?.Value);
            float radius = ParseFloatOrZero(xmlNode.Attribute("radius")?.Value);
            radius = radius != 0f ? radius * scale : 80f * scale;
            float activeTime = ParseFloatOrZero(xmlNode.Attribute("activeTime")?.Value);
            if (activeTime == 0f)
            {
                activeTime = 3f;
            }
            int index = ParseIntOrZero(xmlNode.Attribute("index")?.Value);
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
