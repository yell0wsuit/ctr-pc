using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>Verifies the spider's arm/walk/leave state machine.</summary>
    public class SpiderRiderTests
    {
        [Fact]
        public void NewRiderIsDormant()
        {
            SpiderRider rider = new();

            Assert.Equal(SpiderRiderState.Dormant, rider.State);
            Assert.False(rider.IsWalking);
            Assert.True(rider.IsAttached);
        }

        [Fact]
        public void ArmMovesToArmingOnlyWhenARopeReachesACandy()
        {
            SpiderRider rider = new();

            rider.Arm(ropeAttachedToCandy: false);
            Assert.Equal(SpiderRiderState.Dormant, rider.State);

            rider.Arm(ropeAttachedToCandy: true);
            Assert.Equal(SpiderRiderState.Arming, rider.State);
        }

        [Fact]
        public void ArmFalseDisarmsAnArmedRiderBeforeItWalks()
        {
            // A rope whose tail is not a grabbable candy must not start the spider.
            SpiderRider rider = new();
            rider.Arm(ropeAttachedToCandy: true);

            rider.Arm(ropeAttachedToCandy: false);

            Assert.Equal(SpiderRiderState.Dormant, rider.State);
        }

        [Fact]
        public void ActivateStartsWalking()
        {
            SpiderRider rider = new();
            rider.Arm(ropeAttachedToCandy: true);

            rider.Activate();

            Assert.Equal(SpiderRiderState.Walking, rider.State);
            Assert.True(rider.IsWalking);
        }

        [Fact]
        public void ActivateDoesNothingWhileDormant()
        {
            SpiderRider rider = new();

            rider.Activate();

            Assert.Equal(SpiderRiderState.Dormant, rider.State);
        }

        [Fact]
        public void BustStopsAWalkingRiderForGood()
        {
            SpiderRider rider = new();
            rider.Arm(ropeAttachedToCandy: true);
            rider.Activate();

            rider.Bust();

            Assert.Equal(SpiderRiderState.Busted, rider.State);
            Assert.False(rider.IsWalking);
            Assert.False(rider.IsAttached);

            rider.Arm(ropeAttachedToCandy: true);
            Assert.Equal(SpiderRiderState.Busted, rider.State);
        }

        [Fact]
        public void WonRiderHasLeftTheHook()
        {
            SpiderRider rider = new();
            rider.Arm(ropeAttachedToCandy: true);
            rider.Activate();

            rider.Win();

            Assert.Equal(SpiderRiderState.Won, rider.State);
            Assert.False(rider.IsAttached);
        }
    }
}
