using System;
using System.Globalization;

using CutTheRopeDX.Framework.Core;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// The seed behind a player's magic hat colors, kept in a plain text file they can edit.
    /// </summary>
    /// <remarks>
    /// Deliberately its own file rather than an entry in the preferences JSON: the number is meant
    /// to be opened, changed and deleted by hand. Editing it repaints the hats, deleting it draws a
    /// new set. The store puts it in the save directory on desktop and in localStorage in a browser.
    /// </remarks>
    internal static class SockBandSeed
    {
        /// <summary>Name of the seed file.</summary>
        internal const string FileName = "hatseed.txt";

        /// <summary>
        /// Reads the player's seed, drawing and storing a new one when there is nothing usable yet.
        /// </summary>
        /// <param name="store">Where the seed file lives, or <see langword="null"/> when nothing is installed.</param>
        /// <returns>The seed to generate hat colors from.</returns>
        internal static ulong Read(IPreferenceStore store)
        {
            if (store is null)
            {
                // Headless runs persist nothing, and a level full of hats still has to load.
                return FallbackSeed;
            }

            string contents = store.Read(FileName);
            if (contents is not null && ulong.TryParse(
                contents.Trim(),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out ulong stored))
            {
                return stored;
            }

            ulong drawn = Draw();
            store.Write(FileName, drawn.ToString(CultureInfo.InvariantCulture) + Environment.NewLine);
            return drawn;
        }

        /// <summary>Seed used when there is nowhere to keep one.</summary>
        private const ulong FallbackSeed = 0x5CA1AB1E5EEDUL;

        /// <summary>
        /// Draws a fresh seed. The one place real randomness belongs: everything downstream is a
        /// deterministic function of whatever comes out of here.
        /// </summary>
        /// <returns>A newly drawn seed.</returns>
        private static ulong Draw()
        {
            return (ulong)Random.Shared.NextInt64(1L, long.MaxValue);
        }
    }
}
