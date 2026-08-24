using System.Xml.Linq;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>Loads a time-freeze button from XML node data.</summary>
        /// <param name="xmlNode">The XML node describing the switcher.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadPauseSwitcher(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            PauseSwitcher switcher = PauseSwitcher.Create();
            switcher.x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            switcher.y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            switcher.Update(0f);
            pauseSwitchers.Add(switcher);
            pauseSwitcherWaves = PauseSwitcherWaves.Create(SCREEN_WIDTH, SCREEN_HEIGHT);
        }
    }
}
