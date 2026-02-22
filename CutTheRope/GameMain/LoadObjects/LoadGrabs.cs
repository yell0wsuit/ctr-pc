using System;
using System.Xml.Linq;

using CutTheRope.Framework.Core;
using CutTheRope.Framework.Sfe;

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
            float hx = (ParseIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float hy = (ParseIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
            float len = ParseIntOrZero(xmlNode.Attribute("length")?.Value) * scale;
            float grabRadius = ParseFloatOrZero(xmlNode.Attribute("radius")?.Value);
            _ = bool.TryParse(xmlNode.Attribute("wheel")?.Value, out bool wheel);
            _ = bool.TryParse(xmlNode.Attribute("kickable")?.Value, out bool kickable);
            _ = bool.TryParse(xmlNode.Attribute("kicked")?.Value, out bool kicked);
            _ = bool.TryParse(xmlNode.Attribute("invisible")?.Value, out bool invisible);
            float k = ParseFloatOrZero(xmlNode.Attribute("moveLength")?.Value) * scale;
            _ = bool.TryParse(xmlNode.Attribute("moveVertical")?.Value, out bool v);
            float o = ParseFloatOrZero(xmlNode.Attribute("moveOffset")?.Value) * scale;
            _ = bool.TryParse(xmlNode.Attribute("spider")?.Value, out bool spider);
            bool flag = xmlNode.Attribute("part")?.Value == "L";
            _ = bool.TryParse(xmlNode.Attribute("hidePath")?.Value, out bool flag2);
            _ = bool.TryParse(xmlNode.Attribute("bindBulb")?.Value, out bool bindBulb);
            string bulbNumber = xmlNode.Attribute("bulbNumber")?.Value ?? string.Empty;
            _ = bool.TryParse(xmlNode.Attribute("gun")?.Value, out bool gun);
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
                    bool flag3 = (xmlNode.Attribute("path")?.Value ?? string.Empty).StartsWith('R');
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
