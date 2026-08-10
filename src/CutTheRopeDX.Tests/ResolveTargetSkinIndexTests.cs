using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class ResolveTargetSkinIndexTests
    {
        // totalSkinCount = 16 (classic + 15 XML skins), selected = 4 unless noted.

        [Fact]
        public void ZeroDefersToSelectedSkin()
        {
            Assert.Equal(4, OmNomSkinRegistry.ResolveTargetSkinIndex(targetType: 0, selectedSkinIndex: 4, totalSkinCount: 16));
        }

        [Fact]
        public void OneMapsToClassicSlotZero()
        {
            Assert.Equal(0, OmNomSkinRegistry.ResolveTargetSkinIndex(targetType: 1, selectedSkinIndex: 4, totalSkinCount: 16));
        }

        [Fact]
        public void TwoMapsToFirstXmlSkinSlotOne()
        {
            Assert.Equal(1, OmNomSkinRegistry.ResolveTargetSkinIndex(targetType: 2, selectedSkinIndex: 4, totalSkinCount: 16));
        }

        [Fact]
        public void TotalSkinCountMapsToLastSlot()
        {
            Assert.Equal(15, OmNomSkinRegistry.ResolveTargetSkinIndex(targetType: 16, selectedSkinIndex: 4, totalSkinCount: 16));
        }

        [Fact]
        public void AboveRangeDefersToSelectedSkin()
        {
            Assert.Equal(4, OmNomSkinRegistry.ResolveTargetSkinIndex(targetType: 17, selectedSkinIndex: 4, totalSkinCount: 16));
        }

        [Fact]
        public void NegativeDefersToSelectedSkin()
        {
            Assert.Equal(4, OmNomSkinRegistry.ResolveTargetSkinIndex(targetType: -3, selectedSkinIndex: 4, totalSkinCount: 16));
        }

        [Fact]
        public void FallbackReturnsSelectedSkinVerbatim()
        {
            Assert.Equal(7, OmNomSkinRegistry.ResolveTargetSkinIndex(targetType: 0, selectedSkinIndex: 7, totalSkinCount: 16));
        }
    }
}
