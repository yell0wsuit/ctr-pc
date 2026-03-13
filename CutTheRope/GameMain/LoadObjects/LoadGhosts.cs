using System.Xml.Linq;

using static CutTheRope.Helpers.ParsingHelpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
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
