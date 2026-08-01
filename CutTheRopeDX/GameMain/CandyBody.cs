using CutTheRopeDX.Framework.Helpers;
using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.Framework.Visual;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Physical role of a body owned by one logical candy.</summary>
    internal enum CandyBodyRole
    {
        /// <summary>The intact body of a present logical candy.</summary>
        Whole,

        /// <summary>The left body owned by a split logical candy.</summary>
        LeftHalf,

        /// <summary>The right body owned by a split logical candy.</summary>
        RightHalf,
    }

    /// <summary>Owns the physics, visual, and transient presentation state of one candy body.</summary>
    internal sealed class CandyBody
    {
        /// <summary>Initializes a candy body with fixed physics and visual references.</summary>
        /// <param name="point">The constrained physics point.</param>
        /// <param name="role">The body's role within its logical candy.</param>
        /// <param name="visual">The root visual, or <see langword="null"/> for a headless body.</param>
        /// <param name="main">The main visual layer, or <see langword="null"/> when absent.</param>
        /// <param name="top">The top visual layer, or <see langword="null"/> when absent.</param>
        /// <param name="blinkAnimation">The blink animation, or <see langword="null"/> when absent.</param>
        /// <param name="bubbleAnimation">The bubble animation, or <see langword="null"/> when absent.</param>
        /// <param name="ghostBubbleAnimation">The ghost-bubble animation, or <see langword="null"/> when absent.</param>
        internal CandyBody(
            ConstraintedPoint point,
            CandyBodyRole role,
            GameObject visual = null,
            GameObject main = null,
            GameObject top = null,
            Animation blinkAnimation = null,
            Animation bubbleAnimation = null,
            CandyInGhostBubbleAnimation ghostBubbleAnimation = null)
        {
            Point = point;
            Role = role;
            Visual = visual;
            Main = main;
            Top = top;
            BlinkAnimation = blinkAnimation;
            BubbleAnimation = bubbleAnimation;
            GhostBubbleAnimation = ghostBubbleAnimation;
        }

        /// <summary>Gets the constrained physics point for this body.</summary>
        public ConstraintedPoint Point { get; }

        /// <summary>Gets the body's role within its logical candy.</summary>
        public CandyBodyRole Role { get; }

        /// <summary>Gets the root visual, or <see langword="null"/> for a headless body.</summary>
        public GameObject Visual { get; }

        /// <summary>Gets the main visual layer, or <see langword="null"/> when absent.</summary>
        public GameObject Main { get; }

        /// <summary>Gets the top visual layer, or <see langword="null"/> when absent.</summary>
        public GameObject Top { get; }

        /// <summary>Gets the blink animation, or <see langword="null"/> when absent.</summary>
        public Animation BlinkAnimation { get; }

        /// <summary>Gets the normal bubble animation, or <see langword="null"/> when absent.</summary>
        public Animation BubbleAnimation { get; }

        /// <summary>Gets the ghost-bubble animation, or <see langword="null"/> when absent.</summary>
        public CandyInGhostBubbleAnimation GhostBubbleAnimation { get; }

        /// <summary>Gets or sets the bubble currently carrying this body.</summary>
        public GameObject Bubble { get; set; }

        /// <summary>Gets or sets whether <see cref="Bubble"/> belongs to a ghost-transformed bubble.</summary>
        public bool BubbleHasGhost { get; set; }

        /// <summary>Gets or sets residual rope-swing rotation that decays while no rope steers the body.</summary>
        public float ResidualRotation { get; set; }

        /// <summary>Gets or sets whether the water-surface splash has been emitted.</summary>
        public bool Splashes { get; set; }

        /// <summary>Gets or sets whether the body is fully below the water surface.</summary>
        public bool Underwater { get; set; }
    }
}
