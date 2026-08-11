using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class TargetFeedingStateTests
    {
        [Fact]
        public void NewTargetStartsIdleAndUnfed()
        {
            TargetFeedingState feeding = new();

            Assert.Equal(TargetFeedingPhase.Idle, feeding.Phase);
            Assert.False(feeding.IsFed);
            Assert.False(feeding.IsAsleep);
        }

        [Fact]
        public void MouthTimerBelongsToTheMouthOpenPhase()
        {
            TargetFeedingState feeding = new();

            Assert.True(feeding.TryOpenMouth(closeDelay: 1f));
            Assert.Equal(TargetFeedingPhase.MouthOpen, feeding.Phase);
            Assert.Equal(1f, feeding.MouthCloseTime);

            Assert.False(feeding.AdvanceMouthClose(0.5f, candyNearby: false, refreshDelay: 1f));
            Assert.Equal(0.5f, feeding.MouthCloseTime);
            Assert.True(feeding.AdvanceMouthClose(0.5f, candyNearby: false, refreshDelay: 1f));
            Assert.Equal(TargetFeedingPhase.Idle, feeding.Phase);
            Assert.Equal(0f, feeding.MouthCloseTime);
        }

        [Fact]
        public void NearbyCandyRefreshesMouthWithoutCreatingAnotherState()
        {
            TargetFeedingState feeding = new();
            _ = feeding.TryOpenMouth(closeDelay: 1f);

            Assert.False(feeding.AdvanceMouthClose(1f, candyNearby: true, refreshDelay: 1f));

            Assert.Equal(TargetFeedingPhase.MouthOpen, feeding.Phase);
            Assert.Equal(1f, feeding.MouthCloseTime);
        }

        [Fact]
        public void EatingAtomicallyClosesTheMouthAndBeginsChewing()
        {
            TargetFeedingState feeding = new();
            _ = feeding.TryOpenMouth(closeDelay: 1f);

            Assert.True(feeding.TryBeginChewing());

            Assert.Equal(TargetFeedingPhase.Chewing, feeding.Phase);
            Assert.True(feeding.IsFed);
            Assert.False(feeding.IsAsleep);
            Assert.Equal(0f, feeding.MouthCloseTime);
            Assert.False(feeding.TryOpenMouth(closeDelay: 1f));
        }

        [Fact]
        public void IdleTargetCannotSkipTheOpenMouthPhaseAndBeginChewing()
        {
            TargetFeedingState feeding = new();

            Assert.False(feeding.TryBeginChewing());
            Assert.Equal(TargetFeedingPhase.Idle, feeding.Phase);
            Assert.False(feeding.IsFed);
        }

        [Fact]
        public void OnlyChewingCanCompleteDelayedSleep()
        {
            TargetFeedingState feeding = new();
            Assert.False(feeding.TryFallAsleep());

            _ = feeding.TryOpenMouth(closeDelay: 1f);
            Assert.True(feeding.TryBeginChewing());
            Assert.True(feeding.TryFallAsleep());
            Assert.False(feeding.TryFallAsleep());

            Assert.Equal(TargetFeedingPhase.Asleep, feeding.Phase);
            Assert.True(feeding.IsAsleep);
        }
    }
}
