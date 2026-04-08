using System.Globalization;

namespace CutTheRope.Helpers
{
    /// <summary>
    /// Provides safe parsing methods that return default values instead of throwing on invalid input.
    /// </summary>
    public static class ParsingHelpers
    {
        /// <summary>
        /// Parses an integer from a string, returning 0 if the value is <see langword="null"/>, empty, or not a valid integer.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The parsed integer, or 0 if parsing fails.</returns>
        public static int ParseIntOrZero(string value)
        {
            return int.TryParse(value, out int parsed) ? parsed : 0;
        }

        /// <summary>
        /// Parses a floating-point number from a string using invariant culture, returning 0 if the value
        /// is <see langword="null"/>, empty, or not a valid number.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The parsed float, or 0 if parsing fails.</returns>
        public static float ParseFloatOrZero(string value)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) ? parsed : 0f;
        }
    }
}
