using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading the gravity switch button from XML level data
    /// The gravity button allows the player to toggle gravity direction
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads the gravity switch button from XML node data
        /// Creates and positions the gravity toggle button
        /// </summary>
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
