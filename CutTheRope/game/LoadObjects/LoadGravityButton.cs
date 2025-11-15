using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.game
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
            gravityButton.x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            gravityButton.y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            gravityButton.anchor = 18;
        }
    }
}
