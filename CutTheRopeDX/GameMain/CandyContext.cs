using System.Collections.Generic;

using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// One logical candy, created per <c>&lt;candy&gt;</c> element. It owns the carrier and capability
    /// state that answers "what is happening to this candy", while its physics, visuals, and transient
    /// presentation state live on the <see cref="CandyBody"/> instances owned by its
    /// <see cref="CandyLifecycle"/>. The lifecycle decides which bodies exist; win/loss decisions read
    /// <see cref="ToOutcomeView"/> once per context regardless of how many bodies that is.
    /// </summary>
    internal sealed class CandyContext
    {
        /// <summary>Gets the whole physical body owned by this logical candy.</summary>
        internal CandyBody WholeBody { get; }

        /// <summary>
        /// Gets the lifecycle that owns this candy's presence, removal, split,
        /// and transport state.
        /// </summary>
        internal CandyLifecycle Lifecycle { get; }

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

        /// <summary>
        /// True when this candy has no whole body in play right now, because it was permanently
        /// removed, hidden inside a transport session, or replaced by its split halves. The lifecycle
        /// is the only answer: the scene no longer keeps a second, separately maintained flag for the
        /// primary candy.
        /// </summary>
        public bool HasNoWholeBodyInPlay => Lifecycle.Presence != CandyPresence.Present;

        /// <summary>
        /// True when this candy can be grabbed by a mechanical hand right now: it is present and its
        /// capabilities permit it. The <c>Is*</c> form folds in presence; <see cref="CandyCapabilities"/>
        /// flags (e.g. <c>Capabilities.CanBeGrabbedByHand</c>) are the static capability alone.
        /// </summary>
        public bool IsHandGrabbable => !HasNoWholeBodyInPlay && Capabilities.CanBeGrabbedByHand;

        /// <summary>True when this candy can attach to an ant conveyor right now: present and capable.</summary>
        public bool IsAntAttachable => !HasNoWholeBodyInPlay && Capabilities.CanAttachAnts;

        /// <summary>Behavior flags for this candy-like physics body.</summary>
        public CandyCapabilities Capabilities = CandyCapabilities.Candy;

        /// <summary>Optional light-bulb identifier from XML.</summary>
        public string lightBulbNumber;

        /// <summary>Gets the light-bulb visual root when this context owns one.</summary>
        public LightBulb LightBulb => WholeBody.Visual as LightBulb;

        /// <summary>Optional axe identifier from XML.</summary>
        public string axeNumber;

        /// <summary>Blade visual root for Time Travel axe bodies.</summary>
        public Axe axe;

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
        public float InteractionRotation => Capabilities.CanRotateWithRopes
            ? (WholeBody.Main ?? WholeBody.Visual)?.rotation ?? 0f
            : 0f;

        /// <summary>
        /// Distinct visual elements that should receive mechanical-hand catch/restoration effects.
        /// </summary>
        public List<BaseElement> HandCatchVisuals()
        {
            List<BaseElement> visuals = [];
            AddDistinctVisual(visuals, WholeBody.Visual);
            AddDistinctVisual(visuals, WholeBody.Main);
            AddDistinctVisual(visuals, WholeBody.Top);
            return visuals;
        }

        /// <summary>
        /// Base scale for mechanical-hand catch/restoration effects.
        /// </summary>
        public float HandCatchScale
        {
            get
            {
                GameObject root = WholeBody.Visual;
                GameObject main = WholeBody.Main;
                GameObject top = WholeBody.Top;
                return main != null && top != null && main != root && top != root ? 0.71f : 0.9f;
            }
        }

        /// <summary>
        /// Distinct child visual parts that should be normalized when the root carries transformations.
        /// </summary>
        public List<BaseElement> TransformChildVisuals()
        {
            List<BaseElement> visuals = [];
            AddDistinctChildVisual(visuals, WholeBody.Main);
            AddDistinctChildVisual(visuals, WholeBody.Top);
            return visuals;
        }

        private void AddDistinctChildVisual(List<BaseElement> visuals, BaseElement visual)
        {
            if (visual != null && visual != WholeBody.Visual && !visuals.Contains(visual))
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

        /// <summary>
        /// Takes a logical outcome snapshot of this candy for the win/loss decisions, independent
        /// of how many physical bodies it currently has.
        /// </summary>
        /// <returns>
        /// The candy's lifecycle presence and removal reason paired with the capability that decides
        /// whether it counts toward the win condition.
        /// </returns>
        public CandyOutcomeView ToOutcomeView()
        {
            return new CandyOutcomeView(
                Lifecycle.Presence,
                Lifecycle.RemovalReason,
                Capabilities.CanBeEaten,
                Lifecycle.HasFailedSplitHalf);
        }

        /// <summary>
        /// Initializes logical state for a candy that owns the supplied whole physical body.
        /// </summary>
        /// <param name="wholeBody">The whole physical body owned by this candy.</param>
        internal CandyContext(CandyBody wholeBody)
        {
            WholeBody = wholeBody;
            wholeBody.AttachTo(this);
            Lifecycle = CandyLifecycle.CreatePresent(wholeBody);

            if (wholeBody.Visual is LightBulb bulb)
            {
                bulb.AttachTo(this);
            }
        }

        /// <summary>
        /// Replaces this candy's whole body with the two authored halves and takes ownership of their
        /// bodies, so a scene system holding a half can still reach this logical candy.
        /// </summary>
        /// <param name="split">The owned split-half state built from the map's halves.</param>
        /// <returns>
        /// <see langword="true"/> when the lifecycle accepted the split; otherwise,
        /// <see langword="false"/> and no body changes owner.
        /// </returns>
        internal bool TryBeginSplit(SplitCandyState split)
        {
            if (!Lifecycle.TryBeginSplit(split))
            {
                return false;
            }

            split.Left.Body.AttachTo(this);
            split.Right.Body.AttachTo(this);
            return true;
        }
    }
}
