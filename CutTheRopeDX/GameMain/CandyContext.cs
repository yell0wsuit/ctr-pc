using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// All per-candy state for one independent candy. One per <c>&lt;candy&gt;</c> element.
    /// </summary>
    internal sealed class CandyContext
    {
        // Cut the Rope: Time Travel's candy bounding box is 70x70 with a
        // candy↔candy collision radius of 32. Preserve that radius-to-body ratio by scaling it
        // against DX's candy bounding-box width, which already resolves the correct desktop/mobile
        // value, so the hitbox stays locked to the rendered candy size in both physics models.
        private const float TimeTravelCandyBodyWidth = 70f;
        private const float TimeTravelCandyCollisionRadius = 32f;

        private static float DefaultCandyCollisionRadius =>
            GameScene.GetCandyBoundingBox().w * TimeTravelCandyCollisionRadius / TimeTravelCandyBodyWidth;

        /// <summary>Rope-binding key from XML (<c>"first"</c>/<c>"second"</c>); see <see cref="CandyMatch"/>.</summary>
        public string candyNumber;

        /// <summary>Physics point (the engine's "star") that ropes attach to and that gravity acts on.</summary>
        public ConstraintedPoint point;

        /// <summary>Visual container and its layers.</summary>
        public GameObject candy;

        public GameObject candyMain;

        public GameObject candyTop;

        public Animation candyBlink;

        public Animation candyBubbleAnimation;

        public CandyInGhostBubbleAnimation candyGhostBubbleAnimation;

        /// <summary>True once this candy has been eaten/removed.</summary>
        public bool noCandy;

        /// <summary>
        /// Residual rope-swing rotation for this candy, decayed each frame so the candy
        /// coasts to a stop when no rope is actively steering it. candies[0] uses the
        /// singleton <c>lastCandyRotateDelta</c> instead; index 1+ use this field.
        /// </summary>
        public float lastCandyRotateDelta;

        /// <summary>The bubble currently carrying this candy, if any.</summary>
        public GameObject bubble;

        /// <summary>True when <see cref="bubble"/> belongs to a ghost-transformed bubble.</summary>
        public bool bubbleHasGhost;

        /// <summary>True while this candy is captured in a lantern (was the singleton <c>isCandyInLantern</c>).</summary>
        public bool inLantern;

        /// <summary>The sock currently teleporting this candy, if any (was the singleton <c>targetSock</c>).</summary>
        public Sock targetSock;

        /// <summary>Cached exit speed for the in-progress sock teleport (was the singleton <c>savedSockSpeed</c>).</summary>
        public float savedSockSpeed;

        /// <summary>The bamboo tube currently teleporting this candy, if any (was the singleton <c>targetBambooTube</c>).</summary>
        public BambooTube targetBambooTube;

        /// <summary>
        /// True while this candy is mid-teleport through a transporter (bamboo tube or hat/sock).
        /// Bamboo transit also flips <see cref="noCandy"/>, but hat transit does not, so any
        /// "is it teleporting" decision must consult both targets — this is the single source of truth.
        /// </summary>
        public bool InTransport => targetBambooTube != null || targetSock != null;

        /// <summary>The rocket currently flying this candy, if any (was the singleton <c>activeRocket</c>).</summary>
        public Rocket activeRocket;

        /// <summary>True while a rocket is bound to and flying this candy.</summary>
        public bool HasActiveRocket => activeRocket != null;

        /// <summary>
        /// True when this candy can be grabbed by a mechanical hand right now: it is present and its
        /// capabilities permit it. The <c>Is*</c> form folds in presence; <see cref="CandyCapabilities"/>
        /// flags (e.g. <c>Capabilities.CanBeGrabbedByHand</c>) are the static capability alone.
        /// </summary>
        public bool IsHandGrabbable => !noCandy && Capabilities.CanBeGrabbedByHand;

        /// <summary>True when this candy can attach to an ant conveyor right now: present and capable.</summary>
        public bool IsAntAttachable => !noCandy && Capabilities.CanAttachAnts;

        /// <summary>Ant-conveyor segment currently carrying this candy (null if not carried).</summary>
        public AntsPathSegment antSegment;

        /// <summary>Last segment that carried this candy, held during the re-attach cooldown.</summary>
        public AntsPathSegment lastAntSegment;

        /// <summary>Re-attach cooldown timer for this candy's last segment.</summary>
        public float antCooldown;

        /// <summary>True while this candy must leave a segment's external bounds before re-attaching.</summary>
        public bool antWaitForFly;

        /// <summary>
        /// This candy's own carrier marker on <see cref="antSegment"/>. It starts at the candy's
        /// projection onto the segment and advances at the segment speed, so multiple candies ride the
        /// same lane independently, each keeping the spacing it had on entry. Unused when not carried.
        /// </summary>
        public Vector antInteractionPoint;

        /// <summary>Seconds since this candy attached to <see cref="antSegment"/> (carry snap + detach grace).</summary>
        public float antInteractionTime;

        /// <summary>The mechanical hand currently holding this candy, if any (one candy per hand).</summary>
        public MechanicalHand capturingHand;

        /// <summary>True while this candy is the one carried by the active mouse (single-occupancy).</summary>
        public bool carriedByMouse;

        /// <summary>Behavior flags for this candy-like physics body.</summary>
        public CandyCapabilities Capabilities = CandyCapabilities.Candy;

        /// <summary>Optional light-bulb identifier from XML.</summary>
        public string lightBulbNumber;

        /// <summary>Transitional light-bulb visual root while bulb visuals migrate into candy contexts.</summary>
        public LightBulb lightBulb;

        /// <summary>Light radius when this context emits light.</summary>
        public float lightRadius;

        /// <summary>True when this candy-like context contributes to night-level lighting.</summary>
        public bool emitsLight;

        /// <summary>Additive collision radius used when no absolute pair distance is specified.</summary>
        public float? collisionRadius;

        /// <summary>Effective additive collision radius in the current physics model.</summary>
        public float CollisionRadius => collisionRadius ?? DefaultCandyCollisionRadius;

        /// <summary>
        /// Rotation used by interactions that follow this body's visual orientation.
        /// </summary>
        public float InteractionRotation => Capabilities.CanRotateWithRopes ? (candyMain ?? candy)?.rotation ?? 0f : 0f;

        /// <summary>
        /// Distinct visual elements that should receive mechanical-hand catch/restoration effects.
        /// </summary>
        public List<BaseElement> HandCatchVisuals()
        {
            List<BaseElement> visuals = [];
            AddDistinctVisual(visuals, candy);
            AddDistinctVisual(visuals, candyMain);
            AddDistinctVisual(visuals, candyTop);
            return visuals;
        }

        /// <summary>
        /// Base scale for mechanical-hand catch/restoration effects.
        /// </summary>
        public float HandCatchScale =>
            candyMain != null && candyTop != null && candyMain != candy && candyTop != candy ? 0.71f : 0.9f;

        /// <summary>
        /// Distinct child visual parts that should be normalized when the root carries transformations.
        /// </summary>
        public List<BaseElement> TransformChildVisuals()
        {
            List<BaseElement> visuals = [];
            AddDistinctChildVisual(visuals, candyMain);
            AddDistinctChildVisual(visuals, candyTop);
            return visuals;
        }

        private void AddDistinctChildVisual(List<BaseElement> visuals, BaseElement visual)
        {
            if (visual != null && visual != candy && !visuals.Contains(visual))
            {
                visuals.Add(visual);
            }
        }

        private static void AddDistinctVisual(List<BaseElement> visuals, BaseElement visual)
        {
            if (visual != null && !visuals.Contains(visual))
            {
                visuals.Add(visual);
            }
        }

        /// <summary>Optional absolute collision distance used for pairs involving this context.</summary>
        public float? collisionDistanceOverride;

        /// <summary>Edge-detect flag: candy is breaking the water surface (splash played once).</summary>
        public bool splashes;

        /// <summary>Edge-detect flag: candy is fully below the water surface.</summary>
        public bool underwater;

        /// <summary>Snapshot for the pure decision helpers.</summary>
        public CandyView ToView()
        {
            return new CandyView(point.pos, noCandy, InTransport, Capabilities);
        }
    }
}
