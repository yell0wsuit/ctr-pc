using System;
using System.Reflection;

using CutTheRopeDX.Framework.Media;

using Microsoft.Xna.Framework.Media;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class SongLoopWorkaroundTests
    {
        [Fact]
        public void Scheduler_WaitsForDecodedTailToFinishPlaying()
        {
            SongLoopScheduler scheduler = new();
            scheduler.Schedule(
                duration: TimeSpan.FromSeconds(64),
                position: TimeSpan.FromSeconds(63.5));

            Assert.False(scheduler.Advance(TimeSpan.FromMilliseconds(499), isPlaying: true));
            Assert.True(scheduler.Advance(TimeSpan.FromMilliseconds(1), isPlaying: true));
        }

        [Fact]
        public void Scheduler_DoesNotAdvanceWhileMusicIsPaused()
        {
            SongLoopScheduler scheduler = new();
            scheduler.Schedule(
                duration: TimeSpan.FromSeconds(64),
                position: TimeSpan.FromSeconds(63.5));

            Assert.False(scheduler.Advance(TimeSpan.FromSeconds(1), isPlaying: false));
            Assert.False(scheduler.Advance(TimeSpan.FromMilliseconds(499), isPlaying: true));
            Assert.True(scheduler.Advance(TimeSpan.FromMilliseconds(1), isPlaying: true));
        }

        [Fact]
        public void Scheduler_ReusesFirstTailEstimateAcrossLoops()
        {
            SongLoopScheduler scheduler = new();
            scheduler.Schedule(
                duration: TimeSpan.FromSeconds(64),
                position: TimeSpan.FromSeconds(63.5));
            Assert.True(scheduler.Advance(TimeSpan.FromMilliseconds(500), isPlaying: true));

            scheduler.Schedule(
                duration: TimeSpan.FromSeconds(64),
                position: TimeSpan.FromSeconds(63.4));

            Assert.False(scheduler.Advance(TimeSpan.FromMilliseconds(499), isPlaying: true));
            Assert.True(scheduler.Advance(TimeSpan.FromMilliseconds(1), isPlaying: true));
        }

        [Fact]
        public void Installer_ReplacesMonoGameCompletionHandler()
        {
            using Song song = Song.FromUri(
                "missing",
                new Uri("file:///cut-the-rope-dx-missing-test-song.ogg"));
            completionCalled = false;
            EventHandler replacement = OnCompletion;

            bool installed = MonoGameSongCompletionWorkaround.TryInstall(song, replacement);

            Assert.True(installed);
            FieldInfo field = typeof(Song).GetField(
                "DonePlaying",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Delegate handler = Assert.IsAssignableFrom<Delegate>(field?.GetValue(song));
            _ = handler.DynamicInvoke(song, EventArgs.Empty);
            Assert.True(completionCalled);
        }

        private static void OnCompletion(object sender, EventArgs args)
        {
            completionCalled = true;
        }

        private static bool completionCalled;
    }
}
