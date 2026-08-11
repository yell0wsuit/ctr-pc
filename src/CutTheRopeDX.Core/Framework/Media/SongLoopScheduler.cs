using System;

namespace CutTheRopeDX.Framework.Media
{
    /// <summary>
    /// Tracks the decoded song tail that MonoGame has queued but not played yet.
    /// </summary>
    internal sealed class SongLoopScheduler
    {
        private static readonly TimeSpan DecoderBufferDuration = TimeSpan.FromMilliseconds(250);

        /// <summary>
        /// Starts waiting for the two decoded buffers MonoGame leaves queued at EOF.
        /// </summary>
        public void Schedule(TimeSpan duration)
        {
            long partialBufferTicks = duration.Ticks % DecoderBufferDuration.Ticks;
            remaining = DecoderBufferDuration +
                (partialBufferTicks == 0
                    ? DecoderBufferDuration
                    : TimeSpan.FromTicks(partialBufferTicks));
            scheduled = true;
        }

        /// <summary>
        /// Advances the wait while playback is active.
        /// </summary>
        /// <returns><see langword="true"/> once the queued tail has had time to play.</returns>
        public bool Advance(TimeSpan elapsed, bool isPlaying)
        {
            if (!scheduled || !isPlaying)
            {
                return false;
            }

            remaining -= elapsed;
            if (remaining > TimeSpan.Zero)
            {
                return false;
            }

            scheduled = false;
            return true;
        }

        /// <summary>
        /// Cancels a pending loop restart.
        /// </summary>
        public void Cancel()
        {
            scheduled = false;
            remaining = TimeSpan.Zero;
        }

        private TimeSpan remaining;
        private bool scheduled;
    }
}
