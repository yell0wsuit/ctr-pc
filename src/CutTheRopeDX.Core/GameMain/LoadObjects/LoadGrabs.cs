using System;
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
            // `breakable` defaults to true (a normal, finger-cuttable rope). Only an explicit
            // breakable="false" marks a chain, matching the original (it calls setUnBreakable when
            // the attribute is not "true").
            bool breakable = GetBoolAttribute(xmlNode, "breakable", defaultValue: true);
            bool axed = HasTrueAttribute(xmlNode, "axed");
            string grabCandyNumber = xmlNode.Attribute("candyNumber")?.Value;
            string grabAxeNumber = AxeGrabBinding.ResolveAxeNumber(
                grabCandyNumber,
                xmlNode.Attribute("axeNumber")?.Value,
                axed);
            Grab grab = new();
            grab.initial_x = grab.x = hx;
            grab.initial_y = grab.y = hy;
            grab.initial_rotation = 0f;
            HookModifiers modifiers = HookModifiers.None;
            if (invisible)
            {
                modifiers |= HookModifiers.Invisible;
            }
            if (!breakable)
            {
                modifiers |= HookModifiers.ChainAnchor;
            }
            grab.Modifiers = modifiers;
            if (spider)
            {
                grab.SetSpider();
            }
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

            // One place decides which axes this grab has. Everything below only builds visuals for
            // the axes it was given.
            GrabAxes axes = GrabAxisResolver.Resolve(
                new GrabAxisRequest(
                    Gun: gun,
                    Wheel: wheel,
                    Kickable: kickable,
                    Radius: grabRadius,
                    MoveLength: k,
                    HasMover: grab.mover != null,
                    MoveVertical: v,
                    MoveOffset: o,
                    AnchorX: hx,
                    AnchorY: hy),
                startsKicked: kicked);
            grab.Source = axes.Source;
            grab.Motion = axes.Motion;
            grab.Mount = axes.Mount;
            grab.Wheel = axes.Wheel;

            if (grab.Source is PreAttachedSource)
            {
                ConstraintedPoint constraintedPoint;
                CandyContext targetAxe = grabAxeNumber != null ? FindAxeByNumber(grabAxeNumber) : null;
                CandyContext targetCandy = targetAxe == null && grabCandyNumber != null ? FindCandyByNumber(grabCandyNumber) : null;
                // Single-candy / split-candy behavior: the primary candy's split state, built
                // from the same metadata pass, says which half a part="L"/"R" grab binds to.
                SplitCandyState split = candies[0].Lifecycle.Split;
                ConstraintedPoint authoredHalf = split == null ? null
                    : flag ? split.Left.Body.Point : split.Right.Body.Point;
                if (bindBulb)
                {
                    CandyContext bulb = FindLightEmitterByNumber(bulbNumber);
                    constraintedPoint = bulb != null ? bulb.WholeBody.Point : authoredHalf ?? star;
                }
                else if (targetAxe != null)
                {
                    constraintedPoint = targetAxe.WholeBody.Point;
                }
                else if (targetCandy != null)
                {
                    // Multi-candy: bind to the candy named by candyNumber.
                    constraintedPoint = targetCandy.WholeBody.Point;
                }
                else
                {
                    constraintedPoint = authoredHalf ?? star;
                }

                // A part="L"/"R" grab binds to a half, so the owner lookup has to resolve halves too;
                // an unowned point (no candy at all) simply carries no lantern state.
                CandyContext ropeTarget = CandyForPointOrNull(constraintedPoint);
                if (NormalRopeLoad.ShouldCreate(ropeTarget?.Lifecycle.Attachments.InLantern == true))
                {
                    Bungee bungee = new Bungee().InitWithHeadAtXYTailAtTXTYandLength(null, hx, hy, constraintedPoint, constraintedPoint.pos.X, constraintedPoint.pos.Y, len);
                    bungee.bungeeAnchor.pin = bungee.bungeeAnchor.pos;
                    if (!breakable)
                    {
                        // breakable="false" is a chain: it renders as a chain and can only be cut by the
                        // axe (the original's single `isUnBreakable` flag). `axed`/axeNumber is purely a
                        // bind target and does not make the rope axe-only.
                        bungee.SetCutOnlyByAxe();
                    }
                    grab.SetRope(bungee);
                    ropes.Register(bungee, grab);
                    if (grab.Mount?.IsMounted == false)
                    {
                        grab.Mount.Kick(grab);
                    }
                }
            }
            grab.CreateAxisVisuals();
            grab.CreateRailVisuals();
            if (grab.GunSource != null && grab.GunSource.Arrow != null)
            {
                SplitCandyState split = candies[0].Lifecycle.Split;
                ConstraintedPoint constraintedPoint = split == null ? star
                    : flag ? split.Left.Body.Point : split.Right.Body.Point;
                Vector vector = VectSub(Vect(grab.x, grab.y), constraintedPoint.pos);
                grab.GunSource.Arrow.rotation = RADIANS_TO_DEGREES(VectAngleNormalized(vector));
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

        /// <summary>Finds the axe whose <c>axeNumber</c> matches, or null. See <see cref="CandyMatch"/>.</summary>
        private CandyContext FindAxeByNumber(string number)
        {
            for (int i = 0; i < candies.Count; i++)
            {
                if (CandyMatch.Matches(candies[i].axeNumber, number))
                {
                    return candies[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Reads a boolean attribute by local name, allowing older imported Time Travel names as aliases.
        /// </summary>
        /// <param name="node">XML node to inspect.</param>
        /// <param name="name">Attribute local name.</param>
        /// <returns><see langword="true"/> when the attribute exists and parses true.</returns>
        private static bool HasTrueAttribute(XElement node, string name)
        {
            return GetBoolAttribute(node, name, defaultValue: false);
        }

        /// <summary>
        /// Reads a boolean attribute by local name, returning <paramref name="defaultValue"/> when the
        /// attribute is absent. Allows imported Time Travel names as aliases.
        /// </summary>
        /// <param name="node">XML node to inspect.</param>
        /// <param name="name">Attribute local name.</param>
        /// <param name="defaultValue">Value returned when the attribute is not present.</param>
        /// <returns>The parsed boolean, or <paramref name="defaultValue"/> when absent.</returns>
        private static bool GetBoolAttribute(XElement node, string name, bool defaultValue)
        {
            foreach (XAttribute attribute in node.Attributes())
            {
                if (attribute.Name.LocalName == name)
                {
                    return IsTruthy(attribute.Value);
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Parses imported boolean-like values used by level XML.
        /// </summary>
        /// <param name="value">Attribute value.</param>
        /// <returns><see langword="true"/> for <c>true</c> or <c>1</c>.</returns>
        private static bool IsTruthy(string value)
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
        }

    }
}
