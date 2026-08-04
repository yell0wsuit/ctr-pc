using CutTheRopeDX.Desktop;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class QuadIndexPatternTests
    {
        [Fact]
        public void FirstQuadMatchesTriangleStripDecomposition()
        {
            short[] indices = QuadIndexPattern.Build(1);
            // Strip vertices 0,1,2,3 decompose to triangles (0,1,2) and (2,1,3).
            Assert.Equal(new short[] { 0, 1, 2, 2, 1, 3 }, indices);
        }

        [Fact]
        public void SecondQuadIsRebasedByFourVertices()
        {
            short[] indices = QuadIndexPattern.Build(2);
            Assert.Equal(new short[] { 4, 5, 6, 6, 5, 7 }, indices[6..12]);
        }

        [Fact]
        public void FullCapacityStaysWithinSixteenBitRange()
        {
            short[] indices = QuadIndexPattern.Build(QuadIndexPattern.MaxQuads);
            Assert.Equal(QuadIndexPattern.MaxQuads * 6, indices.Length);
            Assert.Equal((short)((QuadIndexPattern.MaxQuads * 4) - 1), indices[^1]);
        }
    }
}
