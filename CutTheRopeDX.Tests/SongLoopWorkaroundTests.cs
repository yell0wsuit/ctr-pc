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

        [Fact]
        public void InstallerReplacesMonoGameCompletionHandler()
        {
            FieldInfo field = typeof(Song).GetField(
                "DonePlaying",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                // The workaround targets a field that only MonoGame's native framework has; the DesktopGL
                // build carries neither the field nor the buffer-flushing bug it compensates for, and
                // TryInstall reports false there so the game keeps MediaPlayer's own looping. Constructing
                // the Song below would fail on that build regardless, since its Song.FromUri opens the file
                // eagerly and this one deliberately does not exist.
                Assert.False(MonoGameSongCompletionWorkaround.TryInstall(
                    Song.FromUri("missing", new Uri("file:///cut-the-rope-dx-missing-test-song.ogg")),
                    OnCompletion));
                return;
            }

            using Song song = Song.FromUri(
                "missing",
                new Uri("file:///cut-the-rope-dx-missing-test-song.ogg"));
            completionCalled = false;
            EventHandler replacement = OnCompletion;

            bool installed = MonoGameSongCompletionWorkaround.TryInstall(song, replacement);

            Assert.True(installed);
            Delegate handler = Assert.IsAssignableFrom<Delegate>(field.GetValue(song));
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
