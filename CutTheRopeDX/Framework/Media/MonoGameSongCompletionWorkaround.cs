using System;
using System.Reflection;

using Microsoft.Xna.Framework.Media;

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

    /// <summary>
    /// Replaces MonoGame 3.8.5's native Song completion callback so MediaPlayer
    /// does not flush the final queued audio buffers before they play.
    /// </summary>
    /// <remarks>
    /// This should be removed once MonoGame resolves the issue.
    /// </remarks>
    internal static class MonoGameSongCompletionWorkaround
    {
        /// <summary>
        /// Replaces the completion handler installed by <see cref="MediaPlayer"/>.
        /// Returns false when MonoGame's internal implementation no longer matches,
        /// allowing callers to retain its default behavior.
        /// </summary>
        public static bool TryInstall(Song song, EventHandler replacement)
        {
            ArgumentNullException.ThrowIfNull(song);
            ArgumentNullException.ThrowIfNull(replacement);

            FieldInfo completionField = typeof(Song).GetField(
                "DonePlaying",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (completionField?.FieldType == null)
            {
                return false;
            }

            try
            {
                Delegate handler = Delegate.CreateDelegate(
                    completionField.FieldType,
                    replacement.Target,
                    replacement.Method);
                completionField.SetValue(song, handler);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (FieldAccessException)
            {
                return false;
            }
        }
    }
}
