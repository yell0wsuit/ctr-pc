using System.Xml.Linq;

using CutTheRope.Framework.Sfe;
using CutTheRope.Helpers;
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
            float x = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            float y = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            float litRadius = (string.IsNullOrEmpty(xmlNode.AttributeAsNSString("litRadius")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("litRadius"), CultureInfo.InvariantCulture)) * scale;
            string bulbNumber = xmlNode.AttributeAsNSString("bulbNumber");

            ConstraintedPoint constraint = new();
            constraint.SetWeight(1f);
            constraint.disableGravity = false;
            constraint.pos = Vect(x, y);

            LightBulb bulb = new(litRadius, constraint, bulbNumber);
            lightBulbs.Add(bulb);
        }
    }
}
