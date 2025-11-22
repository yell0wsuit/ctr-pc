using System.Xml.Linq;

using CutTheRope.Helpers;

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
            float x = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            float y = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            float angle = xmlNode.AttributeAsNSString("angle").FloatValue();
            SteamTube steamTube = new SteamTube().InitWithPositionAngle(Vect(x, y), angle, scale);
            _ = tubes.AddObject(steamTube);
        }
    }
}
