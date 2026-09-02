using CutTheRopeDX.Framework;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Covers the perceptual color math the hat band palette measures separation with.
    /// </summary>
    public class PerceptualColorTests
    {
        /// <summary>
        /// Sharma, Wu and Dalal's published CIEDE2000 test data. Every implementation of the
        /// formula is expected to reproduce these, and they exercise the parts that are easy to
        /// get wrong: the hue-angle wrap, the blue-region rotation term, and near-neutral colors.
        /// </summary>
        [Theory]
        [InlineData(50.0, 2.6772, -79.7751, 50.0, 0.0, -82.7485, 2.0425)]
        [InlineData(50.0, 3.1571, -77.2803, 50.0, 0.0, -82.7485, 2.8615)]
        [InlineData(50.0, -1.3802, -84.2814, 50.0, 0.0, -82.7485, 1.0000)]
        [InlineData(50.0, 0.0, 0.0, 50.0, -1.0, 2.0, 2.3669)]
        [InlineData(50.0, 2.49, -0.001, 50.0, -2.49, 0.0009, 7.1792)]
        [InlineData(50.0, 2.5, 0.0, 73.0, 25.0, -18.0, 27.1492)]
        [InlineData(50.0, 2.5, 0.0, 56.0, -27.0, -3.0, 31.9030)]
        [InlineData(60.2574, -34.0099, 36.2677, 60.4626, -34.1751, 39.4387, 1.2644)]
        [InlineData(22.7233, 20.0904, -46.6940, 23.0331, 14.9730, -42.5619, 2.0373)]
        [InlineData(2.0776, 0.0795, -1.1350, 0.9033, -0.0636, -0.5514, 0.9082)]
        public void DeltaE2000MatchesThePublishedTestData(
            double l1, double a1, double b1,
            double l2, double a2, double b2,
            double expected)
        {
            double got = PerceptualColor.DeltaE2000(new LabColor(l1, a1, b1), new LabColor(l2, a2, b2));

            Assert.Equal(expected, got, 4);
        }

        [Fact]
        public void DeltaE2000IsSymmetric()
        {
            LabColor red = new(50.0, 2.5, 0.0);
            LabColor blue = new(73.0, 25.0, -18.0);

            Assert.Equal(
                PerceptualColor.DeltaE2000(red, blue),
                PerceptualColor.DeltaE2000(blue, red),
                10);
        }
    }
}
