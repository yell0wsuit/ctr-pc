using System;

namespace CutTheRopeDX.GameMain
{
    /// <summary>
    /// Matches a rope's <c>candyNumber</c> to a candy's <c>candyNumber</c> by string identity.
    /// </summary>
    internal static class CandyMatch
    {
        /// <summary>Returns true when both keys are non-null and equal (case-insensitive, trimmed).</summary>
        public static bool Matches(string a, string b)
        {
            return a != null
                    && b != null
                    && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
