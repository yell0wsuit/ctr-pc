using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class LanternCaptureTests
    {
        // baseline: inactive lantern, group free, candy present & not already in, in range -> capture
        [Fact]
        public void ShouldCapture_TrueForEligibleCandyInRange()
        {
            Assert.True(LanternCapture.ShouldCapture(
                lanternInactive: true, groupOccupied: false, candyPresent: true,
                candyAlreadyInLantern: false, inRange: true));
        }

        [Fact]
        public void ShouldCapture_FalseWhenGroupAlreadyOccupied()
        {
            // single-occupancy: a candy already captured blocks a second.
            Assert.False(LanternCapture.ShouldCapture(true, groupOccupied: true, true, false, true));
        }

        [Fact]
        public void ShouldCapture_FalseWhenLanternActive()
        {
            Assert.False(LanternCapture.ShouldCapture(lanternInactive: false, false, true, false, true));
        }

        [Fact]
        public void ShouldCapture_FalseWhenCandyMissingOrAlreadyIn()
        {
            Assert.False(LanternCapture.ShouldCapture(true, false, candyPresent: false, false, true));
            Assert.False(LanternCapture.ShouldCapture(true, false, true, candyAlreadyInLantern: true, true));
        }

        [Fact]
        public void ShouldCapture_FalseWhenOutOfRange()
        {
            Assert.False(LanternCapture.ShouldCapture(true, false, true, false, inRange: false));
        }
    }
}
