using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class NightSleepStateTests
    {
        [Fact]
        public void DarknessStartsFallingAsleepWithAllTimingOwnedTogether()
        {
            NightSleepState sleep = new();

            Assert.Equal(NightSleepTransition.FellAsleep, sleep.ObserveAwake(
                isAwake: false,
                pulseDelay: 0.75f,
                pulseBaseY: 42f));

            Assert.Equal(NightSleepPhase.FallingAsleep, sleep.Phase);
            Assert.False(sleep.IsAwake);
            Assert.Equal(0.75f, sleep.PulseDelay);
            Assert.Equal(0f, sleep.PulseTime);
            Assert.Equal(42f, sleep.PulseBaseY);
            Assert.Equal(0.9f, sleep.SoundTime);
        }

        [Fact]
        public void FallingAsleepAdvancesIntoPulsing()
        {
            NightSleepState sleep = new();
            _ = sleep.ObserveAwake(false, pulseDelay: 0.5f, pulseBaseY: 42f);

            sleep.AdvancePulse(0.25f);
            Assert.Equal(NightSleepPhase.FallingAsleep, sleep.Phase);
            sleep.AdvancePulse(0.25f);

            Assert.Equal(NightSleepPhase.Pulsing, sleep.Phase);
            Assert.Equal(0f, sleep.PulseDelay);
            Assert.Equal(0.25f, sleep.PulseTime);

            sleep.AdvancePulse(0.1f);
            Assert.Equal(0.35f, sleep.PulseTime, precision: 3);
        }

        [Fact]
        public void WakingAtomicallyClearsSleepPresentationData()
        {
            NightSleepState sleep = new();
            _ = sleep.ObserveAwake(false, pulseDelay: 0f, pulseBaseY: 42f);
            sleep.AdvancePulse(0.1f);
            _ = sleep.SetOverlayVisible(true, feedingAsleep: false);
            _ = sleep.AdvanceSound(4.1f, interval: 4f);

            Assert.Equal(NightSleepTransition.Woke, sleep.ObserveAwake(
                isAwake: true,
                pulseDelay: 0f,
                pulseBaseY: 0f));

            Assert.Equal(NightSleepPhase.Waking, sleep.Phase);
            Assert.True(sleep.IsAwake);
            Assert.Equal(0f, sleep.PulseDelay);
            Assert.Equal(0f, sleep.PulseTime);
            Assert.Equal(0f, sleep.PulseBaseY);
            Assert.Equal(0f, sleep.SoundTime);
            Assert.False(sleep.OverlayVisible);

            Assert.Equal(NightSleepTransition.None, sleep.ObserveAwake(true, 0f, 0f));
            Assert.Equal(NightSleepPhase.Awake, sleep.Phase);
        }

        [Fact]
        public void TerminalPresentationCleanupPreservesDarkSleepPhase()
        {
            NightSleepState sleep = new();
            _ = sleep.ObserveAwake(false, pulseDelay: 0f, pulseBaseY: 42f);
            sleep.AdvancePulse(0.1f);
            _ = sleep.SetOverlayVisible(true, feedingAsleep: false);
            _ = sleep.AdvanceSound(1f, interval: 4f);

            sleep.ClearPresentation();

            Assert.Equal(NightSleepPhase.Pulsing, sleep.Phase);
            Assert.False(sleep.IsAwake);
            Assert.Equal(0f, sleep.PulseTime);
            Assert.Equal(0f, sleep.PulseDelay);
            Assert.Equal(0f, sleep.PulseBaseY);
            Assert.Equal(0f, sleep.SoundTime);
            Assert.False(sleep.OverlayVisible);
        }

        [Fact]
        public void OverlayAndSoundUpdatesAreEdgeTriggered()
        {
            NightSleepState sleep = new();

            Assert.False(sleep.SetOverlayVisible(true, feedingAsleep: false));
            _ = sleep.ObserveAwake(false, pulseDelay: 0f, pulseBaseY: 0f);
            Assert.True(sleep.SetOverlayVisible(true, feedingAsleep: false));
            Assert.False(sleep.SetOverlayVisible(true, feedingAsleep: false));
            Assert.False(sleep.AdvanceSound(3f, interval: 4f));
            Assert.True(sleep.AdvanceSound(1.1f, interval: 4f));
            Assert.Equal(0f, sleep.SoundTime);
        }

        [Fact]
        public void FeedingSleepCanOwnTheOverlayOutsideANightSleepPhase()
        {
            NightSleepState sleep = new();

            Assert.True(sleep.SetOverlayVisible(true, feedingAsleep: true));
            Assert.True(sleep.OverlayVisible);
        }
    }
}
