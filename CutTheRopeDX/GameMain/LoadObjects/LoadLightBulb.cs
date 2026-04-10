using System.Xml.Linq;

using CutTheRopeDX.Framework.Physics;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a light bulb object from XML node data
        /// </summary>
        /// <param name="xmlNode">The XML node describing the light bulb.</param>
        /// <param name="scale">The level scale factor applied to object coordinates and radius.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadLightBulb(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float litRadius = ParseFloatOrZero(xmlNode.Attribute("litRadius")?.Value) * scale;
            string bulbNumber = xmlNode.Attribute("bulbNumber")?.Value ?? string.Empty;

            ConstraintedPoint constraint = new();
            constraint.SetWeight(1f);
            constraint.disableGravity = false;
            constraint.pos = Vect(x, y);

            LightBulb bulb = new(litRadius, constraint, bulbNumber);
            lightBulbs.Add(bulb);
        }
    }
}
