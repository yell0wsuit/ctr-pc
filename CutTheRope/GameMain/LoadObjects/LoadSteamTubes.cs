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
            float x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float angle = string.IsNullOrEmpty(xmlNode.Attribute("angle")?.Value) ? 0f : float.Parse(xmlNode.Attribute("angle")?.Value, CultureInfo.InvariantCulture);
            SteamTube steamTube = new SteamTube().InitWithPositionAngle(Vect(x, y), angle, scale);
            tubes.Add(steamTube);
        }
    }
}
