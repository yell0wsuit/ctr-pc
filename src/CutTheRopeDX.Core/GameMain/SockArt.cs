using System;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Which art a magic hat draws, given its teleport group and the season.
    /// </summary>
    /// <remarks>
    /// The loader and <see cref="LevelResourceScanner"/> both answer this question, and they have
    /// to answer it the same way: a level that lists one texture on the loading screen and draws
    /// another reads the missing one off disk on the game thread the first time a hat appears.
    /// </remarks>
    internal static class SockArt
    {
        /// <summary>The texture resource a hat of this group draws from.</summary>
        /// <param name="group">Teleport group the hat belongs to.</param>
        /// <param name="isXmas">Whether the seasonal theme is running.</param>
        /// <returns>The texture resource name.</returns>
        internal static string TextureFor(int group, bool isXmas)
        {
            // The Christmas art draws two socks and stops there, so a group past them has nothing
            // seasonal to wear and falls back to the magic hat, whose band any group can generate.
            return isXmas && !WearsGeneratedBand(group)
                ? Resources.Img.ObjSock
                : Resources.Img.ObjHat;
        }

        /// <summary>Whether a hat of this group wears a band generated from the player's seed.</summary>
        /// <param name="group">Teleport group the hat belongs to.</param>
        /// <returns><see langword="true"/> when the shipped art bakes no color for this group.</returns>
        internal static bool WearsGeneratedBand(int group)
        {
            return NormalizeGroup(group) >= SockBandPalette.AuthoredCount;
        }

        /// <summary>The base frame, and the band pattern authored over it, for this group.</summary>
        /// <param name="group">Teleport group the hat belongs to.</param>
        /// <returns>Zero-based pattern index.</returns>
        internal static int PatternFor(int group)
        {
            return NormalizeGroup(group) % SockBandPalette.AuthoredCount;
        }

        /// <summary>
        /// A group as the art can use it. Level XML is data, and a negative group would otherwise
        /// index off the front of every lookup here.
        /// </summary>
        /// <param name="group">Teleport group as the level authored it.</param>
        /// <returns>The group, never below zero.</returns>
        internal static int NormalizeGroup(int group)
        {
            return Math.Max(group, 0);
        }
    }
}
