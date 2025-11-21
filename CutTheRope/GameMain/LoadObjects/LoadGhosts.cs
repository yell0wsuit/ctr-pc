using System.Xml.Linq;

using CutTheRope.Helpers;

namespace CutTheRope.GameMain
{
    internal sealed partial class GameScene
    {
        private void LoadGhost(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float px = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            float py = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            float grabRadius = xmlNode.AttributeAsNSString("radius").FloatValue();
            if (grabRadius != -1f)
            {
                grabRadius *= scale;
            }
            float bouncerAngle = xmlNode.AttributeAsNSString("angle").FloatValue();
            bool useGrab = xmlNode.AttributeAsNSString("grab").BoolValue();
            bool useBubble = xmlNode.AttributeAsNSString("bubble").BoolValue();
            bool useBouncer = xmlNode.AttributeAsNSString("bouncer").BoolValue();
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
            _ = ghosts.AddObject(ghost);
            EnsureCandyGhostBubbleAnimations();
        }
    }
}
