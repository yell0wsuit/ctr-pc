using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// The rocket steals from nobody: when the candy is already held (hand or mouse), the rocket
    /// binds with zero rest length straight into FLY at the candy's position, coexisting with the
    /// holder until it lets go. The decision is per candy — a holder on ANOTHER candy must not
    /// flip this candy's bind path (the old global handHoldingCandy did exactly that).
    /// </summary>
    public class RocketBindPathTests
    {
        [Fact]
        public void UsesDirectFlyPath_TrueWhenAHandHoldsThisCandy()
        {
            Assert.True(RocketBindPath.UsesDirectFlyPath(handHoldsThisCandy: true, mouseCarriesThisCandy: false));
        }

        [Fact]
        public void UsesDirectFlyPath_TrueWhenTheMouseCarriesThisCandy()
        {
            Assert.True(RocketBindPath.UsesDirectFlyPath(handHoldsThisCandy: false, mouseCarriesThisCandy: true));
        }

        [Fact]
        public void UsesDirectFlyPath_FalseForAFreeCandy()
        {
            // Free candy uses the classic DIST reel-in path.
            Assert.False(RocketBindPath.UsesDirectFlyPath(handHoldsThisCandy: false, mouseCarriesThisCandy: false));
        }
    }
}
