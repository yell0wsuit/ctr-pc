using System;
using System.Xml.Linq;
using System.Globalization;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadGhost(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float py = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float grabRadius = string.IsNullOrEmpty(xmlNode.Attribute("radius")?.Value) ? 0f : float.Parse(xmlNode.Attribute("radius")?.Value, CultureInfo.InvariantCulture);
            if (grabRadius != -1f)
            {
                grabRadius *= scale;
            }
            float bouncerAngle = string.IsNullOrEmpty(xmlNode.Attribute("angle")?.Value) ? 0f : float.Parse(xmlNode.Attribute("angle")?.Value, CultureInfo.InvariantCulture);
            bool useGrab = "true".Equals(xmlNode.Attribute("grab")?.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            bool useBubble = "true".Equals(xmlNode.Attribute("bubble")?.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            bool useBouncer = "true".Equals(xmlNode.Attribute("bouncer")?.Value ?? string.Empty, StringComparison.OrdinalIgnoreCase);
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
