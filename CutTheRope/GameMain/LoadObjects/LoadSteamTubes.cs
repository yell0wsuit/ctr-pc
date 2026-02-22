using System.Xml.Linq;

using CutTheRope.Helpers;
using System.Globalization;

namespace CutTheRope.GameMain
{
    /// <summary>
    /// Handles loading steam tube objects from XML level data.
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a steam tube from XML node data and positions it in the scene.
        /// </summary>
        private void LoadSteamTube(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            float y = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            float angle = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("angle")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("angle"), CultureInfo.InvariantCulture);
            SteamTube steamTube = new SteamTube().InitWithPositionAngle(Vect(x, y), angle, scale);
            tubes.Add(steamTube);
        }
    }
}
