using System;
using System.Reflection;

using Microsoft.Xna.Framework.Media;

namespace CutTheRopeDX.Framework.Media
{
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
