using System;
using System.Xml.Linq;

using CutTheRope.Helpers;
using System.Globalization;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadGhost(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            float py = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            float grabRadius = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("radius")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("radius"), CultureInfo.InvariantCulture);
            if (grabRadius != -1f)
            {
                grabRadius *= scale;
            }
            float bouncerAngle = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("angle")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("angle"), CultureInfo.InvariantCulture);
            bool useGrab = "true".Equals(xmlNode.AttributeAsNSString("grab"), StringComparison.OrdinalIgnoreCase);
            bool useBubble = "true".Equals(xmlNode.AttributeAsNSString("bubble"), StringComparison.OrdinalIgnoreCase);
            bool useBouncer = "true".Equals(xmlNode.AttributeAsNSString("bouncer"), StringComparison.OrdinalIgnoreCase);
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
