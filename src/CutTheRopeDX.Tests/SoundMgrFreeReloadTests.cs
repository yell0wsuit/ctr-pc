using System;
using System.Collections.Generic;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    /// <summary>
    /// Pins the free-then-reload contract between <see cref="SoundMgr"/> and an audio backend:
    /// gameplay sounds are freed when a session ends and loaded again for the next level, so a
    /// released effect must never be handed back a second time. A backend that leaves a disposed
    /// effect in its own cache turns one quit-to-menu into silence for the rest of the process.
    /// </summary>
    public sealed class SoundMgrFreeReloadTests
    {
        [Fact]
        public void FreeingASoundReleasesTheBackendEffect()
        {
            _ = HeadlessGame.Boot();
            SoundMgr manager = Application.SharedSoundMgr();
            CountingAudioBackend backend = new();
            SoundMgr.SetBackend(backend);

            try
            {
                FakeSoundEffect first = (FakeSoundEffect)manager.GetSound(Resources.Snd.Bouncer);
                Assert.NotNull(first);
                Assert.False(first.Released);

                manager.FreeSound(Resources.Snd.Bouncer);

                Assert.True(first.Released);
            }
            finally
            {
                manager.StopAllSounds();
                SoundMgr.SetBackend(null);
            }
        }

        [Fact]
        public void ReloadingAFreedSoundYieldsAUsableEffect()
        {
            _ = HeadlessGame.Boot();
            SoundMgr manager = Application.SharedSoundMgr();
            CountingAudioBackend backend = new();
            SoundMgr.SetBackend(backend);

            try
            {
                ISoundEffect first = manager.GetSound(Resources.Snd.Bouncer);
                manager.FreeSound(Resources.Snd.Bouncer);

                FakeSoundEffect second = (FakeSoundEffect)manager.GetSound(Resources.Snd.Bouncer);

                Assert.NotNull(second);
                Assert.NotSame(first, second);
                Assert.False(second.Released);

                // The whole point of reloading: it can still be played.
                Assert.NotNull(second.CreateInstance());
                Assert.Equal(2, backend.LoadCount);
            }
            finally
            {
                manager.StopAllSounds();
                SoundMgr.SetBackend(null);
            }
        }

        [Fact]
        public void FreeingOneSoundLeavesOtherLoadedSoundsUsable()
        {
            _ = HeadlessGame.Boot();
            SoundMgr manager = Application.SharedSoundMgr();
            CountingAudioBackend backend = new();
            SoundMgr.SetBackend(backend);

            try
            {
                FakeSoundEffect bouncer = (FakeSoundEffect)manager.GetSound(Resources.Snd.Bouncer);
                FakeSoundEffect electric = (FakeSoundEffect)manager.GetSound(Resources.Snd.Electric);

                manager.FreeSound(Resources.Snd.Bouncer);

                Assert.True(bouncer.Released);
                Assert.False(electric.Released);
                Assert.Same(electric, manager.GetSound(Resources.Snd.Electric));
                Assert.NotNull(electric.CreateInstance());
            }
            finally
            {
                manager.StopAllSounds();
                SoundMgr.SetBackend(null);
            }
        }

        [Fact]
        public void FreeingALoopedSoundStopsOnlyItsOwnInstances()
        {
            _ = HeadlessGame.Boot();
            SoundMgr manager = Application.SharedSoundMgr();
            CountingAudioBackend backend = new();
            SoundMgr.SetBackend(backend);

            try
            {
                ISoundInstance loop = manager.PlaySoundLooped(Resources.Snd.Electric);
                ISoundInstance other = manager.PlaySoundLooped(Resources.Snd.TransporterMove);
                Assert.Equal(AudioPlaybackState.Playing, loop.State);
                Assert.Equal(AudioPlaybackState.Playing, other.State);

                manager.FreeSound(Resources.Snd.Electric);

                Assert.Equal(AudioPlaybackState.Stopped, loop.State);
                Assert.Equal(AudioPlaybackState.Playing, other.State);
            }
            finally
            {
                manager.StopAllSounds();
                SoundMgr.SetBackend(null);
            }
        }

        /// <summary>
        /// A backend whose released effects stay released, so a test can tell a fresh load from
        /// a corpse handed back out of a cache.
        /// </summary>
        private sealed class CountingAudioBackend : IAudioBackend
        {
            private readonly Dictionary<string, int> loadsByPath = [];

            public int LoadCount { get; private set; }

            public ISoundEffect LoadSound(string contentPath)
            {
                LoadCount++;
                loadsByPath[contentPath] = loadsByPath.GetValueOrDefault(contentPath) + 1;
                return new FakeSoundEffect();
            }

            public IMusicTrack LoadMusic(string contentPath)
            {
                throw new NotSupportedException();
            }

            public void PlayMusic(IMusicTrack track, bool repeating)
            {
            }

            public void StopMusic()
            {
            }

            public void PauseMusic()
            {
            }

            public void ResumeMusic()
            {
            }

            public AudioPlaybackState MusicState => AudioPlaybackState.Stopped;

            public bool TryInstallSongCompletionCallback(IMusicTrack track, EventHandler<EventArgs> onDecoderFinished)
            {
                return false;
            }
        }

        private sealed class FakeSoundEffect : ISoundEffect
        {
            public bool Released { get; private set; }

            public ISoundInstance CreateInstance()
            {
                ObjectDisposedException.ThrowIf(Released, this);
                return new FakeSoundInstance();
            }

            public void Dispose()
            {
                Released = true;
            }
        }

        private sealed class FakeSoundInstance : ISoundInstance
        {
            public bool IsLooped { get; set; }

            public float Volume { get; set; } = 1f;

            public AudioPlaybackState State { get; private set; } = AudioPlaybackState.Stopped;

            public void Play()
            {
                State = AudioPlaybackState.Playing;
            }

            public void Stop()
            {
                State = AudioPlaybackState.Stopped;
            }

            public void Pause()
            {
                State = AudioPlaybackState.Paused;
            }

            public void Resume()
            {
                State = AudioPlaybackState.Playing;
            }

            public void Dispose()
            {
                State = AudioPlaybackState.Stopped;
            }
        }
    }
}
