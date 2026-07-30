using CutTheRopeDX.Desktop;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class BlendParamsSnapshotTests
    {
        [Fact]
        public void DefaultConstructedSnapshotIsDefault()
        {
            BlendParams blend = new();
            Assert.Equal(BlendParams.BlendType.Default, blend.Snapshot());
        }

        [Fact]
        public void SrcAlphaInvSrcAlphaMapsToSourceAlphaInverseSourceAlpha()
        {
            BlendParams blend = new(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            Assert.Equal(BlendParams.BlendType.SourceAlpha_InverseSourceAlpha, blend.Snapshot());
        }

        [Fact]
        public void OneInvSrcAlphaMapsToOneInverseSourceAlpha()
        {
            BlendParams blend = new(BlendingFactor.GLONE, BlendingFactor.GLONEMINUSSRCALPHA);
            Assert.Equal(BlendParams.BlendType.One_InverseSourceAlpha, blend.Snapshot());
        }

        [Fact]
        public void SrcAlphaOneMapsToSourceAlphaOne()
        {
            BlendParams blend = new(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
            Assert.Equal(BlendParams.BlendType.SourceAlpha_One, blend.Snapshot());
        }

        [Fact]
        public void DisabledBlendSnapshotsAsDefault()
        {
            BlendParams blend = new(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONEMINUSSRCALPHA);
            blend.Disable();
            Assert.Equal(BlendParams.BlendType.Default, blend.Snapshot());
        }

        [Fact]
        public void ReEnabledBlendSnapshotsAsItsFactors()
        {
            BlendParams blend = new(BlendingFactor.GLSRCALPHA, BlendingFactor.GLONE);
            blend.Disable();
            blend.Enable();
            Assert.Equal(BlendParams.BlendType.SourceAlpha_One, blend.Snapshot());
        }

        [Fact]
        public void UnmappedFactorPairSnapshotsAsUnknown()
        {
            BlendParams blend = new(BlendingFactor.GLONE, BlendingFactor.GLONE);
            Assert.Equal(BlendParams.BlendType.Unknown, blend.Snapshot());
        }
    }
}
