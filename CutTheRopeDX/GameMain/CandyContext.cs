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
        private const float DefaultCandyCollisionRadius = 32f;

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

        /// <summary>The rocket currently flying this candy, if any (was the singleton <c>activeRocket</c>).</summary>
        public Rocket activeRocket;

        /// <summary>Ant-conveyor segment currently carrying this candy (null if not carried).</summary>
        public AntsPathSegment antSegment;

        /// <summary>Last segment that carried this candy, held during the re-attach cooldown.</summary>
        public AntsPathSegment lastAntSegment;

        /// <summary>Re-attach cooldown timer for this candy's last segment.</summary>
        public float antCooldown;

        /// <summary>True while this candy must leave a segment's external bounds before re-attaching.</summary>
        public bool antWaitForFly;

        /// <summary>The mechanical hand currently holding this candy, if any (one candy per hand).</summary>
        public MechanicalHand capturingHand;

        /// <summary>True while this candy is the one carried by the active mouse (single-occupancy).</summary>
        public bool carriedByMouse;

        /// <summary>Behavior flags for this candy-like physics body.</summary>
        public CandyCapabilities Capabilities = CandyCapabilities.Candy;

        /// <summary>Light radius when this context emits light.</summary>
        public float lightRadius;

        /// <summary>True when this candy-like context contributes to night-level lighting.</summary>
        public bool emitsLight;

        /// <summary>Additive collision radius used when no absolute pair distance is specified.</summary>
        public float collisionRadius = DefaultCandyCollisionRadius;

        /// <summary>Optional absolute collision distance used for pairs involving this context.</summary>
        public float? collisionDistanceOverride;

        /// <summary>Edge-detect flag: candy is breaking the water surface (splash played once).</summary>
        public bool splashes;

        /// <summary>Edge-detect flag: candy is fully below the water surface.</summary>
        public bool underwater;

        /// <summary>Snapshot for the pure decision helpers.</summary>
        public CandyView ToView()
        {
            return new CandyView(point.pos, noCandy, targetSock != null || targetBambooTube != null, Capabilities);
        }
    }
}
