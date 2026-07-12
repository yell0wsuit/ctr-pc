using System.Xml.Linq;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Physics;

using static CutTheRopeDX.Helpers.ParsingHelpers;

namespace CutTheRopeDX.GameMain
{
    internal sealed partial class GameScene
    {
        /// <summary>
        /// Loads a grab/rope object from XML node data
        /// Handles spider and bee variants, path-based movement, and rope physics
        /// </summary>
        /// <param name="xmlNode">The XML node describing the grab.</param>
        /// <param name="scale">The level scale factor applied to object coordinates.</param>
        /// <param name="offsetX">The base X offset applied to loaded objects.</param>
        /// <param name="offsetY">The base Y offset applied to loaded objects.</param>
        /// <param name="mapOffsetX">The additional map X offset applied during loading.</param>
        /// <param name="mapOffsetY">The additional map Y offset applied during loading.</param>
        private void LoadGrab(XElement xmlNode, float scale, float offsetX, float offsetY, int mapOffsetX, int mapOffsetY)
        {
            float hx = (ParseCoordinateIntOrZero(xmlNode.Attribute("x")?.Value) * scale) + offsetX + mapOffsetX;
            float hy = (ParseCoordinateIntOrZero(xmlNode.Attribute("y")?.Value) * scale) + offsetY + mapOffsetY;
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
            string grabCandyNumber = xmlNode.Attribute("candyNumber")?.Value;
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
                ConstraintedPoint constraintedPoint;
                CandyContext targetCandy = grabCandyNumber != null ? FindCandyByNumber(grabCandyNumber) : null;
                if (targetCandy != null)
                {
                    // Multi-candy: bind to the candy named by candyNumber.
                    grab.candyNumber = 0;
                    constraintedPoint = targetCandy.point;
                }
                else
                {
                    // Single-candy / split-candy behavior.
                    grab.candyNumber = twoParts == 2 ? 0 : flag ? 1 : 2;
                    constraintedPoint = star;
                    if (bindBulb)
                    {
                        CandyContext bulb = FindLightEmitterByNumber(bulbNumber);
                        if (bulb != null)
                        {
                            constraintedPoint = bulb.point;
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
                }

                CandyContext ropeTarget = CandyForPoint(constraintedPoint);
                if (NormalRopeLoad.ShouldCreate(ropeTarget.inLantern))
                {
                    Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, hx, hy, constraintedPoint, constraintedPoint.pos.X, constraintedPoint.pos.Y, len);
                    bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                    grab.SetRope(bungee);
                    if (grab.kicked)
                    {
                        bungee.bungeeAnchor.pin = Vect(-1f, -1f);
                        bungee.bungeeAnchor.SetWeight(0.1f);
                    }
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

        /// <summary>Finds the candy whose <c>candyNumber</c> matches, or null. See <see cref="CandyMatch"/>.</summary>
        private CandyContext FindCandyByNumber(string number)
        {
            for (int i = 0; i < candies.Count; i++)
            {
                if (CandyMatch.Matches(candies[i].candyNumber, number))
                {
                    return candies[i];
                }
            }
            return null;
        }

    }
}
