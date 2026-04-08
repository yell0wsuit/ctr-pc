using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a ghost object from XML node data.
        /// </summary>
        /// <param name="xmlNode">The XML node describing the ghost.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadGhost(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float py = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float grabRadius = ParseFloatOrZero(xmlNode.Attribute("radius")?.Value);
            if (grabRadius != -1f)
            {
                grabRadius *= scale;
            }
            float bouncerAngle = ParseFloatOrZero(xmlNode.Attribute("angle")?.Value);
            _ = bool.TryParse(xmlNode.Attribute("grab")?.Value, out bool useGrab);
            _ = bool.TryParse(xmlNode.Attribute("bubble")?.Value, out bool useBubble);
            _ = bool.TryParse(xmlNode.Attribute("bouncer")?.Value, out bool useBouncer);
            int possibleStatesMask = (useBouncer ? 8 : 0) | (useBubble ? 2 : 0) | (useGrab ? 4 : 0);
            Ghost ghost = new Ghost().InitWithPositionPossibleStatesMaskGrabRadiusBouncerAngleBubblesBungeesBouncers(
                Vect(px, py),
                possibleStatesMask,
                grabRadius,
                bouncerAngle,
                bubbles,
                bungees,
                bouncers,
                this);
            ghosts.Add(ghost);
            EnsureCandyGhostBubbleAnimations();
        }
    }
}
