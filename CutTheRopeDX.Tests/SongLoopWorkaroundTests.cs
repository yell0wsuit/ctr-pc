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
#if MONOGAME_DESKTOPGL
            // The workaround targets a private field that only MonoGame's native framework has, and the
            // buffer-flushing bug it compensates for is native-only too. On DesktopGL there is nothing to
            // install, and the game keeps MediaPlayer's own looping. Asserted rather than skipped so the
            // reason stays checked, and without constructing a Song: DesktopGL's Song.FromUri opens the
            // file eagerly, and the one below deliberately does not exist.
            Assert.Null(typeof(Song).GetField("DonePlaying", BindingFlags.Instance | BindingFlags.NonPublic));
#else
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
            Delegate handler = Assert.IsType<Delegate>(field?.GetValue(song), exactMatch: false);
            _ = handler.DynamicInvoke(song, EventArgs.Empty);
            Assert.True(completionCalled);
#endif
        }

        private static void OnCompletion(object sender, EventArgs args)
        {
            completionCalled = true;
        }

        private static bool completionCalled;
    }
}
