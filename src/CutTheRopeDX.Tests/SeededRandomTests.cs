using CutTheRopeDX.Framework;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the generator behind seeded hat colors.
    /// </summary>
    public class SeededRandomTests
    {
        /// <summary>
        /// The exact sequence is part of the contract: a player's seed has to produce the same hats
        /// on desktop and in the browser, and has to keep producing them after this code is touched.
        /// These values come from an independent xorshift64* implementation.
        /// </summary>
        [Theory]
        [InlineData(1UL, 0x47E4CE4B896CDD1DUL, 0xABCFA6A8E079651DUL, 0xB9D10D8FEB731F57UL)]
        [InlineData(12345UL, 0x9857FB32C9EFB5E4UL, 0xC0CEBA4B4A71BCE4UL, 0x1399CE5B8ADB52C4UL)]
        public void ProducesTheDocumentedSequence(ulong seed, ulong first, ulong second, ulong third)
        {
            SeededRandom random = new(seed);

            Assert.Equal(first, random.NextULong());
            Assert.Equal(second, random.NextULong());
            Assert.Equal(third, random.NextULong());
        }

        [Fact]
        public void TheSameSeedReplaysTheSameSequence()
        {
            SeededRandom first = new(2024UL);
            SeededRandom second = new(2024UL);

            for (int i = 0; i < 16; i++)
            {
                Assert.Equal(first.NextULong(), second.NextULong());
            }
        }

        [Fact]
        public void NextDoubleStaysInTheUnitInterval()
        {
            SeededRandom random = new(7UL);

            for (int i = 0; i < 512; i++)
            {
                double value = random.NextDouble();
                Assert.InRange(value, 0.0, 1.0);
                Assert.NotEqual(1.0, value);
            }
        }

        [Fact]
        public void ASeedOfZeroStillGeneratesValues()
        {
            // Xorshift is stuck forever on an all-zero state, and a fresh save is exactly where a
            // zero seed would come from, so the constructor has to move off it.
            SeededRandom random = new(0UL);

            Assert.NotEqual(0UL, random.NextULong());
            Assert.NotEqual(0UL, random.NextULong());
        }
    }
}
