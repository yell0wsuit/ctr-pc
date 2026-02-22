using System.Xml.Linq;

using CutTheRope.Framework.Sfe;
using System.Globalization;

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
            float x = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty)) * scale) + offsetX + mapOffsetX;
            float y = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty)) * scale) + offsetY + mapOffsetY;
            float litRadius = (string.IsNullOrEmpty(xmlNode.Attribute("litRadius")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("litRadius")?.Value ?? string.Empty, CultureInfo.InvariantCulture)) * scale;
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
