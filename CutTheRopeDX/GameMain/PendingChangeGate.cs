using System;
using System.Threading;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Collapses a burst of change notifications into a single pending event, released once the source falls quiet.
    /// </summary>
    /// <remarks>
    /// <see cref="NotifyChanged"/> is safe to call from a background thread; <see cref="TryConsume"/> is
    /// expected to be called from the game loop.
    /// </remarks>
    /// <param name="quietPeriod">How long the source must be idle before a change is released.</param>
    internal sealed class PendingChangeGate(TimeSpan quietPeriod)
    {
        /// <summary>
        /// Records that the watched source changed.
        /// </summary>
        /// <param name="nowUtc">Current UTC time.</param>
        public void NotifyChanged(DateTime nowUtc)
        {
            lock (sync)
            {
                pending = true;
                lastChangeUtc = nowUtc;
            }
        }

        /// <summary>
        /// Takes the pending change if the source has been quiet long enough.
        /// </summary>
        /// <param name="nowUtc">Current UTC time.</param>
        /// <returns><see langword="true"/> when a change was released; otherwise <see langword="false"/>.</returns>
        public bool TryConsume(DateTime nowUtc)
        {
            lock (sync)
            {
                if (!pending || nowUtc - lastChangeUtc < quietPeriod)
                {
                    return false;
                }

                pending = false;
                return true;
            }
        }

        /// <summary>Guards <see cref="pending"/> and <see cref="lastChangeUtc"/> across threads.</summary>
        private readonly Lock sync = new();

        /// <summary>Whether a change is waiting to be released.</summary>
        private bool pending;

        /// <summary>UTC timestamp of the most recent change notification.</summary>
        private DateTime lastChangeUtc;
    }
}
