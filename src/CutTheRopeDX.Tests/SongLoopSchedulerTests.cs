using System;

using CutTheRopeDX.Framework.Media;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class SongLoopSchedulerTests
    {
        [Fact]
        public void SchedulerWaitsForDecodedTailToFinishPlaying()
        {
            SongLoopScheduler scheduler = new();
            scheduler.Schedule(duration: TimeSpan.FromSeconds(64));

            Assert.False(scheduler.Advance(TimeSpan.FromMilliseconds(499), isPlaying: true));
            Assert.True(scheduler.Advance(TimeSpan.FromMilliseconds(1), isPlaying: true));
        }

        [Fact]
        public void SchedulerDoesNotAdvanceWhileMusicIsPaused()
        {
            SongLoopScheduler scheduler = new();
            scheduler.Schedule(duration: TimeSpan.FromSeconds(64));

            Assert.False(scheduler.Advance(TimeSpan.FromSeconds(1), isPlaying: false));
            Assert.False(scheduler.Advance(TimeSpan.FromMilliseconds(499), isPlaying: true));
            Assert.True(scheduler.Advance(TimeSpan.FromMilliseconds(1), isPlaying: true));
        }

        [Fact]
        public void SchedulerUsesSameDecoderTailAcrossLoops()
        {
            SongLoopScheduler scheduler = new();
            scheduler.Schedule(duration: TimeSpan.FromSeconds(64));
            Assert.True(scheduler.Advance(TimeSpan.FromMilliseconds(500), isPlaying: true));

            scheduler.Schedule(duration: TimeSpan.FromSeconds(64));

            Assert.False(scheduler.Advance(TimeSpan.FromMilliseconds(499), isPlaying: true));
            Assert.True(scheduler.Advance(TimeSpan.FromMilliseconds(1), isPlaying: true));
        }

        [Fact]
        public void SchedulerUsesDecoderTailForCachedMenuSong()
        {
            SongLoopScheduler scheduler = new();

            scheduler.Schedule(duration: TimeSpan.FromSeconds(60));

            Assert.False(scheduler.Advance(TimeSpan.FromMilliseconds(499), isPlaying: true));
            Assert.True(scheduler.Advance(TimeSpan.FromMilliseconds(1), isPlaying: true));
        }
    }
}
