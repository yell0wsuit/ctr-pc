using System;
using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Sfe;
using CutTheRope.Helpers;
using System.Globalization;

namespace CutTheRope.GameMain
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
            float hx = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("x")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("x"))) * scale) + offsetX + mapOffsetX;
            float hy = ((string.IsNullOrEmpty(xmlNode.AttributeAsNSString("y")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("y"))) * scale) + offsetY + mapOffsetY;
            float len = (string.IsNullOrEmpty(xmlNode.AttributeAsNSString("length")) ? 0 : int.Parse(xmlNode.AttributeAsNSString("length"))) * scale;
            float grabRadius = string.IsNullOrEmpty(xmlNode.AttributeAsNSString("radius")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("radius"), CultureInfo.InvariantCulture);
            bool wheel = xmlNode.AttributeAsNSString("wheel") == "true";
            bool kickable = xmlNode.AttributeAsNSString("kickable") == "true";
            bool kicked = xmlNode.AttributeAsNSString("kicked") == "true";
            bool invisible = xmlNode.AttributeAsNSString("invisible") == "true";
            float k = (string.IsNullOrEmpty(xmlNode.AttributeAsNSString("moveLength")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("moveLength"), CultureInfo.InvariantCulture)) * scale;
            bool v = xmlNode.AttributeAsNSString("moveVertical") == "true";
            float o = (string.IsNullOrEmpty(xmlNode.AttributeAsNSString("moveOffset")) ? 0f : float.Parse(xmlNode.AttributeAsNSString("moveOffset"), CultureInfo.InvariantCulture)) * scale;
            bool spider = xmlNode.AttributeAsNSString("spider") == "true";
            bool flag = xmlNode.AttributeAsNSString("part") == "L";
            bool flag2 = xmlNode.AttributeAsNSString("hidePath") == "true";
            bool bindBulb = xmlNode.AttributeAsNSString("bindBulb") == "true";
            string bulbNumber = xmlNode.AttributeAsNSString("bulbNumber");
            bool gun = xmlNode.AttributeAsNSString("gun") == "true";
            Grab grab = new();
            grab.initial_x = grab.x = hx;
            grab.initial_y = grab.y = hy;
            grab.initial_rotation = 0f;
            grab.wheel = wheel;
            grab.gun = gun;
            grab.kickable = kickable;
            grab.kicked = kicked;
            grab.invisible = invisible;
            grab.SetSpider(spider);
            grab.ParseMover(xmlNode);
            if (grab.mover != null)
            {
                grab.SetBee();
                if (!flag2)
                {
                    int pollenPathStep = 3;
                    bool flag3 = xmlNode.AttributeAsNSString("path").StartsWith("R");
                    for (int l = 0; l < grab.mover.pathLen - 1; l++)
                    {
                        if (!flag3 || l % pollenPathStep == 0)
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
            if (grabRadius != -1f)
            {
                grabRadius *= scale;
            }
            if (grabRadius == -1f && !gun)
            {
                ConstraintedPoint constraintedPoint = star;
                if (bindBulb)
                {
                    LightBulb bulb = FindLightBulbForBinding(bulbNumber);
                    if (bulb != null)
                    {
                        constraintedPoint = bulb.constraint;
                    }
                    else if (twoParts != 2)
                    {
                        constraintedPoint = flag ? starL : starR;
                    }
                }
                else if (twoParts != 2)
                {
                    constraintedPoint = flag ? starL : starR;
                }
                Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, hx, hy, constraintedPoint, constraintedPoint.pos.X, constraintedPoint.pos.Y, len);
                bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                grab.SetRope(bungee);
                if (grab.kicked)
                {
                    bungee.bungeeAnchor.pin = Vect(-1f, -1f);
                    bungee.bungeeAnchor.SetWeight(0.1f);
                }
            }
            grab.SetRadius(grabRadius);
            grab.SetMoveLengthVerticalOffset(k, v, o);
            if (grab.gun && grab.gunArrow != null)
            {
                ConstraintedPoint constraintedPoint = star;
                if (twoParts != 2)
                {
                    constraintedPoint = flag ? starL : starR;
                }
                Vector vector = VectSub(Vect(grab.x, grab.y), constraintedPoint.pos);
                grab.gunArrow.rotation = RADIANS_TO_DEGREES(VectAngleNormalized(vector));
            }
            bungees.Add(grab);
        }

        private LightBulb FindLightBulbForBinding(string bulbNumber)
        {
            if (lightBulbs.Count == 0)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(bulbNumber))
            {
                for (int i = 0; i < lightBulbs.Count; i++)
                {
                    LightBulb bulb = lightBulbs[i];
                    if (bulb != null && string.Equals(bulb.bulbNumber, bulbNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        return bulb;
                    }
                }
            }
            return lightBulbs[^1];
        }
    }
}
