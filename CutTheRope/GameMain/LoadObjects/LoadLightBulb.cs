using System.Xml.Linq;

using CutTheRope.Framework.Sfe;
using CutTheRope.Helpers;

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
            float x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            float y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            float litRadius = xmlNode.AttributeAsNSString("litRadius").FloatValue() * scale;
            string bulbNumber = xmlNode.AttributeAsNSString("bulbNumber");

            ConstraintedPoint constraint = new();
            constraint.SetWeight(1f);
            constraint.disableGravity = false;
            constraint.pos = Vect(x, y);

            LightBulb bulb = new(litRadius, constraint, bulbNumber);
            _ = lightBulbs.AddObject(bulb);
        }
    }
}
