using System.Xml.Linq;

using CutTheRope.Helpers;
using CutTheRope.iframework.sfe;

namespace CutTheRope.game
{
    /// <summary>
    /// Handles loading grab/hook objects from XML level data
    /// Grabs are rope attachment points and can have spiders or bees
    /// </summary>
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a grab/rope object from XML node data
        /// Handles spider and bee variants, path-based movement, and rope physics
        /// </summary>
        private void LoadGrab(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float hx = (xmlNode.AttributeAsNSString("x").IntValue() * scale) + offsetX + mapOffsetX;
            float hy = (xmlNode.AttributeAsNSString("y").IntValue() * scale) + offsetY + mapOffsetY;
            float len = xmlNode.AttributeAsNSString("length").IntValue() * scale;
            float num12 = xmlNode.AttributeAsNSString("radius").FloatValue();
            bool wheel = xmlNode.AttributeAsNSString("wheel").IsEqualToString("true");
            float k = xmlNode.AttributeAsNSString("moveLength").FloatValue() * scale;
            bool v = xmlNode.AttributeAsNSString("moveVertical").IsEqualToString("true");
            float o = xmlNode.AttributeAsNSString("moveOffset").FloatValue() * scale;
            bool spider = xmlNode.AttributeAsNSString("spider").IsEqualToString("true");
            bool flag = xmlNode.AttributeAsNSString("part").IsEqualToString("L");
            bool flag2 = xmlNode.AttributeAsNSString("hidePath").IsEqualToString("true");
            Grab grab = new();
            grab.initial_x = grab.x = hx;
            grab.initial_y = grab.y = hy;
            grab.initial_rotation = 0f;
            grab.wheel = wheel;
            grab.SetSpider(spider);
            grab.ParseMover(xmlNode);
            if (grab.mover != null)
            {
                grab.SetBee();
                if (!flag2)
                {
                    int num13 = 3;
                    bool flag3 = xmlNode.AttributeAsNSString("path").HasPrefix("R");
                    for (int l = 0; l < grab.mover.pathLen - 1; l++)
                    {
                        if (!flag3 || l % num13 == 0)
                        {
                            pollenDrawer.FillWithPolenFromPathIndexToPathIndexGrab(l, l + 1, grab);
                        }
                    }
                    if (grab.mover.pathLen > 2)
                    {
                        pollenDrawer.FillWithPolenFromPathIndexToPathIndexGrab(0, grab.mover.pathLen - 1, grab);
                    }
                }
            }
            if (num12 != -1f)
            {
                num12 *= scale;
            }
            if (num12 == -1f)
            {
                ConstraintedPoint constraintedPoint = star;
                if (twoParts != 2)
                {
                    constraintedPoint = flag ? starL : starR;
                }
                Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, hx, hy, constraintedPoint, constraintedPoint.pos.x, constraintedPoint.pos.y, len);
                bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                grab.SetRope(bungee);
            }
            grab.SetRadius(num12);
            grab.SetMoveLengthVerticalOffset(k, v, o);
            _ = bungees.AddObject(grab);
        }
    }
}
