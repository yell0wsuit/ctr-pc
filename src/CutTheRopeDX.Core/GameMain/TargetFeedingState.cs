using System;

namespace CutTheRopeDX.GameMain
{
    /// <summary>Mutually exclusive feeding behavior for one Om Nom.</summary>
    internal enum TargetFeedingPhase
    {
        /// <summary>Waiting for a candy to approach.</summary>
        Idle,

        /// <summary>The mouth is open for a nearby candy.</summary>
        MouthOpen,

        /// <summary>A candy was eaten and the chewing animation is active.</summary>
        Chewing,

        /// <summary>Post-eat sleep is active.</summary>
        Asleep,
    }

    /// <summary>Single owner of one Om Nom's feeding phase and mouth-close timer.</summary>
    internal sealed class TargetFeedingState
    {
        /// <summary>Gets the current mutually exclusive feeding phase.</summary>
        public TargetFeedingPhase Phase { get; private set; }

        /// <summary>Gets the remaining time before an open mouth is reconsidered.</summary>
        public float MouthCloseTime { get; private set; }

        /// <summary>Gets whether this target has already consumed a candy.</summary>
        public bool IsFed => Phase is TargetFeedingPhase.Chewing or TargetFeedingPhase.Asleep;

        /// <summary>Gets whether post-eat sleep is active.</summary>
        public bool IsAsleep => Phase == TargetFeedingPhase.Asleep;

        /// <summary>Opens an idle target's mouth and starts its close countdown.</summary>
        /// <param name="closeDelay">Seconds before proximity is checked again.</param>
        /// <returns><see langword="true"/> when the mouth opened.</returns>
        public bool TryOpenMouth(float closeDelay)
        {
            if (Phase != TargetFeedingPhase.Idle)
            {
                return false;
            }

            Phase = TargetFeedingPhase.MouthOpen;
            MouthCloseTime = MathF.Max(0f, closeDelay);
            return true;
        }

        /// <summary>Advances an open mouth and either refreshes or closes it when its timer expires.</summary>
        /// <param name="delta">Elapsed seconds.</param>
        /// <param name="candyNearby">Whether an eligible candy remains close enough.</param>
        /// <param name="refreshDelay">Countdown restored while a candy remains nearby.</param>
        /// <returns><see langword="true"/> only when the mouth closes on this call.</returns>
        public bool AdvanceMouthClose(float delta, bool candyNearby, float refreshDelay)
        {
            if (Phase != TargetFeedingPhase.MouthOpen)
            {
                return false;
            }

            MouthCloseTime = MathF.Max(0f, MouthCloseTime - MathF.Max(0f, delta));
            if (MouthCloseTime > 0f)
            {
                return false;
            }

            if (candyNearby)
            {
                MouthCloseTime = MathF.Max(0f, refreshDelay);
                return false;
            }

            Phase = TargetFeedingPhase.Idle;
            return true;
        }

        /// <summary>Atomically records consumption, closes the mouth, and begins chewing.</summary>
        /// <returns><see langword="true"/> when an unfed target begins chewing.</returns>
        public bool TryBeginChewing()
        {
            if (Phase != TargetFeedingPhase.MouthOpen)
            {
                return false;
            }

            Phase = TargetFeedingPhase.Chewing;
            MouthCloseTime = 0f;
            return true;
        }

        /// <summary>Completes the delayed transition from chewing to post-eat sleep.</summary>
        /// <returns><see langword="true"/> only for the current chewing phase.</returns>
        public bool TryFallAsleep()
        {
            if (Phase != TargetFeedingPhase.Chewing)
            {
                return false;
            }

            Phase = TargetFeedingPhase.Asleep;
            return true;
        }
    }
}
