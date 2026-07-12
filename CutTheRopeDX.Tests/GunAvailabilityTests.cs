using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public class GunAvailabilityTests
    {
        [Fact]
        public void GunIsDisabledAndCannotFireWhileCandyIsInLantern()
        {
            Assert.True(GunAvailability.IsDisabled(candyInLantern: true));
            Assert.False(GunAvailability.CanFire(
                candyPresent: true, candyInLantern: true, gunFired: false, ropeAbsent: true));
        }

        [Fact]
        public void GunBecomesEnabledAndCanFireAfterCandyIsFreed()
        {
            Assert.False(GunAvailability.IsDisabled(candyInLantern: false));
            Assert.True(GunAvailability.CanFire(
                candyPresent: true, candyInLantern: false, gunFired: false, ropeAbsent: true));
        }
    }
}
