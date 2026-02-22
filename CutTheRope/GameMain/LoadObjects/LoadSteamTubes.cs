using System.Xml.Linq;
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
            float x = ((string.IsNullOrEmpty(xmlNode.Attribute("x")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("x")?.Value ?? string.Empty)) * scale) + offsetX + mapOffsetX;
            float y = ((string.IsNullOrEmpty(xmlNode.Attribute("y")?.Value ?? string.Empty) ? 0 : int.Parse(xmlNode.Attribute("y")?.Value ?? string.Empty)) * scale) + offsetY + mapOffsetY;
            float angle = string.IsNullOrEmpty(xmlNode.Attribute("angle")?.Value ?? string.Empty) ? 0f : float.Parse(xmlNode.Attribute("angle")?.Value ?? string.Empty, CultureInfo.InvariantCulture);
            SteamTube steamTube = new SteamTube().InitWithPositionAngle(Vect(x, y), angle, scale);
            tubes.Add(steamTube);
        }
    }
}
