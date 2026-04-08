using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads the gravity switch button from XML node data
        /// Creates and positions the gravity toggle button
        /// </summary>
        /// <param name="xmlNode">The XML node describing the gravity switch.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadGravityButton(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            gravityButton = CreateGravityButtonWithDelegate(this);
            gravityButton.visible = false;
            gravityButton.touchable = false;
            _ = AddChild(gravityButton);
            gravityButton.x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            gravityButton.y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            gravityButton.anchor = 18;
        }
    }
}
