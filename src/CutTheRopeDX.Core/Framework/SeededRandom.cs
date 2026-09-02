namespace CutTheRopeDX.Framework
{
    /// <summary>
    /// A small xorshift64* generator whose sequence is part of the contract.
    /// </summary>
    /// <remarks>
    /// <see cref="System.Random"/> would do the job, except that its algorithm is an implementation
    /// detail the runtime is free to change. Anything a player's seed produces has to look the same
    /// on desktop and in the browser, and has to stay the same across builds, so the generator is
    /// spelled out here instead.
    /// </remarks>
    internal sealed class SeededRandom
    {
        /// <summary>Creates a generator for one seed.</summary>
        /// <param name="seed">Seed value; zero is replaced, since xorshift never leaves that state.</param>
        internal SeededRandom(ulong seed)
        {
            state = seed == 0UL ? DefaultState : seed;
        }

        /// <summary>Draws the next 64-bit value.</summary>
        /// <returns>The next value in the sequence.</returns>
        internal ulong NextULong()
        {
            state ^= state >> 12;
            state ^= state << 25;
            state ^= state >> 27;
            return state * Multiplier;
        }

        /// <summary>Draws the next value in the half-open range [0, 1).</summary>
        /// <returns>The next value in the sequence, scaled to the unit interval.</returns>
        internal double NextDouble()
        {
            // Only the top 53 bits are kept: that is exactly what a double can hold without
            // rounding, so every result is a value the caller can compare against exactly.
            return (NextULong() >> 11) * (1.0 / (1UL << 53));
        }

        /// <summary>Draws the next value in the half-open range [<paramref name="min"/>, <paramref name="max"/>).</summary>
        /// <param name="min">Inclusive lower bound.</param>
        /// <param name="max">Exclusive upper bound.</param>
        /// <returns>The next value in the sequence, scaled to that range.</returns>
        internal double NextDouble(double min, double max)
        {
            return min + (NextDouble() * (max - min));
        }

        private const ulong Multiplier = 0x2545F4914F6CDD1DUL;

        /// <summary>State a zero seed falls back to; any non-zero constant would serve.</summary>
        private const ulong DefaultState = 0x9E3779B97F4A7C15UL;

        private ulong state;
    }
}
