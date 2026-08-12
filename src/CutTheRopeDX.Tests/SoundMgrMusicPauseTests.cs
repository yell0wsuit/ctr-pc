using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.GameMain;

using Xunit;

namespace CutTheRopeDX.Tests
{
    public sealed class SoundMgrMusicPauseTests
    {
        [Fact]
        public void PauseMenuMusicToggleStartsMusicImmediately()
        {
            _ = HeadlessGame.Boot();
            SoundMgr manager = Application.SharedSoundMgr();
            RecordingAudioBackend backend = new();
            bool originalMusicPreference = Preferences.GetBooleanForKey("MUSIC_ON");
            SoundMgr.SetBackend(backend);
            manager.StopAllSounds();
            SoundMgr.StopMusic();

            try
            {
                Preferences.SetBooleanForKey(false, "MUSIC_ON");
                GameController controller = HeadlessGame.LoadLevelWithController(pack: 1, level: 4);
                controller.OnButtonPressed(GameControllerButtonId.Pause);

                controller.OnButtonPressed(GameControllerButtonId.ToggleMusic);

                Assert.True(Preferences.GetBooleanForKey("MUSIC_ON"));
                Assert.Equal(AudioPlaybackState.Playing, backend.MusicState);
                Assert.Equal(0, backend.PauseMusicCalls);

                manager.Pause();
                Assert.Equal(AudioPlaybackState.Paused, backend.MusicState);
                Assert.Equal(1, backend.PauseMusicCalls);

                manager.Unpause();
                Assert.Equal(AudioPlaybackState.Playing, backend.MusicState);
                Assert.Equal(1, backend.ResumeMusicCalls);

                manager.Unpause();
                Assert.Equal(AudioPlaybackState.Playing, backend.MusicState);
                Assert.Equal(1, backend.ResumeMusicCalls);
            }
            finally
            {
                manager.StopAllSounds();
                SoundMgr.StopMusic();
                SoundMgr.SetBackend(null);
                Preferences.SetBooleanForKey(originalMusicPreference, "MUSIC_ON");
            }
        }

        private sealed class RecordingAudioBackend : IAudioBackend
        {
            public int PauseMusicCalls { get; private set; }

            public int ResumeMusicCalls { get; private set; }

            public AudioPlaybackState MusicState { get; private set; } = AudioPlaybackState.Stopped;

            public ISoundEffect LoadSound(string contentPath)
            {
                throw new NotSupportedException();
            }

            public IMusicTrack LoadMusic(string contentPath)
            {
                return new RecordingMusicTrack();
            }

            public void PlayMusic(IMusicTrack track, bool repeating)
            {
                MusicState = AudioPlaybackState.Playing;
            }

            public void StopMusic()
            {
                MusicState = AudioPlaybackState.Stopped;
            }

            public void PauseMusic()
            {
                PauseMusicCalls++;
                MusicState = AudioPlaybackState.Paused;
            }

            public void ResumeMusic()
            {
                ResumeMusicCalls++;
                MusicState = AudioPlaybackState.Playing;
            }

            public bool TryInstallSongCompletionCallback(
                IMusicTrack track,
                EventHandler<EventArgs> onDecoderFinished)
            {
                return false;
            }
        }

        private sealed class RecordingMusicTrack : IMusicTrack
        {
            public TimeSpan Duration => TimeSpan.FromMinutes(1);
        }
    }
}
