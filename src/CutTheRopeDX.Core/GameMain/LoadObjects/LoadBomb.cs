using System.Xml.Linq;

using CutTheRopeDX.Framework.Physics;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a Time Travel bomb from XML node data.
        /// </summary>
        private void LoadBomb(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            string bombNumber = xmlNode.Attribute("bombNumber")?.Value ?? string.Empty;

            ConstraintedPoint point = new();
            point.SetWeight(1f);
            point.disableGravity = false;
            point.pos = Vect(x, y);

            candies.Add(BombVisualFactory.Create(point, bombNumber));
        }
    }
}
