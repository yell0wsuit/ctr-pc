using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Scene systems now iterate physical bodies instead of candy contexts, so the one thing that
    /// still has to distinguish a whole candy from a split half is this table. It pins the legacy
    /// exclusions: a half swings, floats, breaks and pops bubbles like any body, but no carrier ever
    /// picks one up and no Om Nom ever eats one.
    /// </summary>
    public class CandyBodyEligibilityTests
    {
        [Theory]
        [InlineData((int)CandyInteraction.Mouse)]
        [InlineData((int)CandyInteraction.Lantern)]
        [InlineData((int)CandyInteraction.Rocket)]
        [InlineData((int)CandyInteraction.Ants)]
        [InlineData((int)CandyInteraction.Transport)]
        [InlineData((int)CandyInteraction.Hand)]
        [InlineData((int)CandyInteraction.Snail)]
        [InlineData((int)CandyInteraction.Eat)]
        [InlineData((int)CandyInteraction.CandyCollision)]
        public void CarrierAndOutcomeInteractionsAcceptOnlyWholeBodies(int interactionValue)
        {
            CandyInteraction interaction = (CandyInteraction)interactionValue;

            Assert.True(CandyBodyEligibility.Allows(CandyBodyRole.Whole, interaction));
            Assert.False(CandyBodyEligibility.Allows(CandyBodyRole.LeftHalf, interaction));
            Assert.False(CandyBodyEligibility.Allows(CandyBodyRole.RightHalf, interaction));
        }

        [Theory]
        [InlineData((int)CandyInteraction.Physics)]
        [InlineData((int)CandyInteraction.Water)]
        [InlineData((int)CandyInteraction.Pump)]
        [InlineData((int)CandyInteraction.Steam)]
        [InlineData((int)CandyInteraction.Bubble)]
        [InlineData((int)CandyInteraction.Rope)]
        [InlineData((int)CandyInteraction.Star)]
        [InlineData((int)CandyInteraction.Hazard)]
        [InlineData((int)CandyInteraction.Bouncer)]
        [InlineData((int)CandyInteraction.Spider)]
        [InlineData((int)CandyInteraction.OffScreen)]
        [InlineData((int)CandyInteraction.LightCollision)]
        public void PhysicalInteractionsAcceptEveryBodyRole(int interactionValue)
        {
            CandyInteraction interaction = (CandyInteraction)interactionValue;

            Assert.True(CandyBodyEligibility.Allows(CandyBodyRole.Whole, interaction));
            Assert.True(CandyBodyEligibility.Allows(CandyBodyRole.LeftHalf, interaction));
            Assert.True(CandyBodyEligibility.Allows(CandyBodyRole.RightHalf, interaction));
        }

        [Theory]
        [InlineData((int)CandyBodyRole.Whole, (int)CandyInteraction.Mouse, true)]
        [InlineData((int)CandyBodyRole.LeftHalf, (int)CandyInteraction.Mouse, false)]
        [InlineData((int)CandyBodyRole.RightHalf, (int)CandyInteraction.Hazard, true)]
        public void BodyRolePreservesLegacyEligibility(int roleValue, int interactionValue, bool expected)
        {
            bool allowed = CandyBodyEligibility.Allows(
                (CandyBodyRole)roleValue,
                (CandyInteraction)interactionValue);

            Assert.Equal(expected, allowed);
        }
    }
}
