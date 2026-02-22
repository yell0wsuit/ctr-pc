using System.Xml.Linq;

using CutTheRope.Framework.Sfe;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// GameScene.LoadLightBulb - Partial class handling loading of light bulb objects from XML
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a light bulb object from XML node data
        /// </summary>
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
