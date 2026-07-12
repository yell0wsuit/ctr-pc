using System.Xml.Linq;

using CutTheRopeDX.Framework;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a lantern object from XML node data.
        /// </summary>
        /// <param name="xmlNode">The XML node describing the lantern.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadLantern(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float x = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float y = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            _ = bool.TryParse(xmlNode.Attribute("candyCaptured")?.Value, out bool isCandyCaptured);

            Lantern lantern = new Lantern().InitWithPosition(Vect(x, y));
            lantern.ParseMover(xmlNode);
            if (isCandyCaptured)
            {
                candies[0].inLantern = true;
                lantern.CaptureCandy(star);
                candy.x = star.pos.X;
                candy.y = star.pos.Y;
                candy.color = RGBAColor.transparentRGBA;
            }
        }
    }
}
