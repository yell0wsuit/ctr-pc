using CutTheRopeDX.Framework.Physics;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Per-candy mouse ownership. The multi-candy work must decide "does the mouse hold THIS candy"
    /// by physics-point identity, not the old global "does the mouse hold any candy" flag, which
    /// wrongly coupled unrelated candies (a mouse carrying candy A blocked interactions on candy B).
    /// </summary>
    public class MouseOwnershipTests
    {
        [Fact]
        public void CarriesCandyTrueWhenActiveMouseCarriesThisPoint()
        {
            ConstraintedPoint candy = new();
            Assert.True(MouseOwnership.CarriesCandy(carriedByActiveMouse: candy, candyPoint: candy));
        }

        [Fact]
        public void CarriesCandyFalseForADifferentCandyPoint()
        {
            // The mouse carries candy A; candy B must be reported as not-carried so its own
            // rocket bind / rope attach is unaffected. This is the multi-candy isolation invariant.
            ConstraintedPoint candyA = new();
            ConstraintedPoint candyB = new();
            Assert.False(MouseOwnership.CarriesCandy(carriedByActiveMouse: candyA, candyPoint: candyB));
        }

        [Fact]
        public void CarriesCandyFalseWhenMouseCarriesNothing()
        {
            ConstraintedPoint candy = new();
            Assert.False(MouseOwnership.CarriesCandy(carriedByActiveMouse: null, candyPoint: candy));
        }
    }
}
