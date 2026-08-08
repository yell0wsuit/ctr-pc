using CutTheRopeDX.Framework.Core;

using static CutTheRopeDX.Framework.Helpers.CTRMathHelper;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// A hook stuck to the wall by a suction cup. Tapping it detaches the cup, after which the rope
    /// anchor - not the hook - drives the hook's position, and it falls until it re-sticks inside the
    /// map bounds.
    /// <para>
    /// This is deliberately not an <see cref="AnchorMotion"/>: motions answer "who supplies the
    /// position", and a detached cup inverts that relationship rather than being another answer to it.
    /// While detached it overrides whatever motion the hook has.
    /// </para>
    /// </summary>
    /// <param name="startsKicked">
    /// <see langword="true"/> when the level authors the cup as already detached.
    /// </param>
    internal sealed class SuctionMount(bool startsKicked)
    {
        /// <summary>Weight given to the anchor while the cup hangs free.</summary>
        public const float DetachedAnchorWeight = 0.1f;

        /// <summary>Weight restored to the anchor when the cup re-sticks.</summary>
        public const float MountedAnchorWeight = 0.02f;

        /// <summary>Share of the new anchor position the back layer moves to each frame.</summary>
        private const float BackLayerFollow = 0.8f;

        /// <summary>Gets whether the cup is currently stuck to the wall.</summary>
        public bool IsMounted { get; private set; } = !startsKicked;

        /// <summary>Gets the remaining stain decals this cup can leave.</summary>
        public int StainCount { get; private set; } = Grab.MAX_STAINS;

        /// <summary>Gets the elapsed sticking time, or -1 when not trying to stick.</summary>
        public float StickTimer { get; private set; } = -1f;

        /// <summary>Gets whether a platform should drive this hook this frame.</summary>
        public bool FollowsPlatform => IsMounted;

        /// <summary>Detaches the cup and unpins its rope anchor.</summary>
        /// <param name="grab">The hook this mount belongs to.</param>
        public void Kick(Grab grab)
        {
            IsMounted = false;
            StickTimer = -1f;
            if (grab.Rope != null)
            {
                grab.Rope.bungeeAnchor.pin = Vect(-1f, -1f);
                grab.Rope.bungeeAnchor.SetWeight(DetachedAnchorWeight);
            }
        }

        /// <summary>Starts trying to re-stick.</summary>
        public void BeginSticking()
        {
            StickTimer = 0f;
        }

        /// <summary>Abandons the current re-stick attempt.</summary>
        public void CancelSticking()
        {
            StickTimer = -1f;
        }

        /// <summary>Advances the re-stick timer.</summary>
        /// <param name="delta">Elapsed time in seconds.</param>
        /// <returns>
        /// <see langword="true"/> when the stick delay has elapsed and the caller should test whether
        /// the hook is inside the map bounds.
        /// </returns>
        public bool TickSticking(float delta)
        {
            if (StickTimer == -1f)
            {
                return false;
            }

            StickTimer += delta;
            if (StickTimer <= Grab.STICK_DELAY)
            {
                return false;
            }

            StickTimer = -1f;
            return true;
        }

        /// <summary>Re-attaches the cup to the wall and re-pins its rope anchor.</summary>
        /// <param name="grab">The hook this mount belongs to.</param>
        public void Remount(Grab grab)
        {
            IsMounted = true;
            if (grab.Rope != null)
            {
                grab.Rope.bungeeAnchor.pin = grab.Rope.bungeeAnchor.pos;
                grab.Rope.bungeeAnchor.SetWeight(MountedAnchorWeight);
            }
        }

        /// <summary>Consumes one stain decal.</summary>
        /// <param name="alpha">Receives the alpha the decal should be drawn at.</param>
        /// <returns><see langword="true"/> when a stain was available.</returns>
        public bool TakeStain(out float alpha)
        {
            if (StainCount <= 0)
            {
                alpha = 0f;
                return false;
            }

            alpha = StainCount / 10f;
            StainCount--;
            return true;
        }

        /// <summary>
        /// Eases the hook toward the rope anchor for the back draw pass. Position synchronization
        /// deliberately remains in the draw path: headless updates never moved the hook, and the
        /// front draw pass supplies the starting position for the next frame's easing.
        /// </summary>
        /// <param name="grab">The hook this mount belongs to.</param>
        public void SyncBackPosition(Grab grab)
        {
            if (IsMounted || grab.Rope == null)
            {
                return;
            }

            Vector anchor = grab.Rope.bungeeAnchor.pos;
            grab.x = (anchor.X * BackLayerFollow) + (grab.x * (1f - BackLayerFollow));
            grab.y = (anchor.Y * BackLayerFollow) + (grab.y * (1f - BackLayerFollow));
        }

        /// <summary>Snaps the hook to the rope anchor for the front draw pass.</summary>
        /// <param name="grab">The hook this mount belongs to.</param>
        public void SyncFrontPosition(Grab grab)
        {
            if (IsMounted || grab.Rope == null)
            {
                return;
            }

            Vector anchor = grab.Rope.bungeeAnchor.pos;
            grab.x = anchor.X;
            grab.y = anchor.Y;
        }
    }
}
