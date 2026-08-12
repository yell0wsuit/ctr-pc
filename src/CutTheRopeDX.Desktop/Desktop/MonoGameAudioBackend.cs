using System;

using CutTheRopeDX.Framework.Media;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

using XnaMediaPlayer = Microsoft.Xna.Framework.Media.MediaPlayer;

namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// MonoGame implementation of <see cref="IAudioBackend"/>, backed by a
    /// <see cref="ContentManager"/> for loading and MonoGame's <see cref="XnaMediaPlayer"/> for
    /// music playback.
    /// </summary>
    internal sealed class MonoGameAudioBackend(ContentManager contentManager) : IAudioBackend
    {
        /// <summary>Wraps a MonoGame <see cref="SoundEffect"/> as the platform sound effect handle.</summary>
        private sealed class XnaSoundEffect(SoundEffect effect) : ISoundEffect
        {
            public SoundEffect Effect { get; } = effect;

            public ISoundInstance CreateInstance()
            {
                return new XnaSoundInstance(Effect.CreateInstance());
            }

            public void Dispose()
            {
                Effect.Dispose();
            }
        }

        /// <summary>Wraps a MonoGame <see cref="SoundEffectInstance"/> as the platform sound instance handle.</summary>
        private sealed class XnaSoundInstance(SoundEffectInstance instance) : ISoundInstance
        {
            public SoundEffectInstance Instance { get; } = instance;

            public void Play()
            {
                Instance.Play();
            }

            public void Stop()
            {
                Instance.Stop();
            }

            public void Pause()
            {
                Instance.Pause();
            }

            public void Resume()
            {
                Instance.Resume();
            }

            public bool IsLooped
            {
                get => Instance.IsLooped;
                set => Instance.IsLooped = value;
            }

            public float Volume
            {
                get => Instance.Volume;
                set => Instance.Volume = value;
            }

            public AudioPlaybackState State => Instance.State switch
            {
                SoundState.Playing => AudioPlaybackState.Playing,
                SoundState.Paused => AudioPlaybackState.Paused,
                SoundState.Stopped => AudioPlaybackState.Stopped,
                _ => AudioPlaybackState.Stopped,
            };

            public void Dispose()
            {
                Instance.Dispose();
            }
        }

        /// <summary>Wraps a MonoGame <see cref="Song"/> as the platform music track handle.</summary>
        private sealed class XnaMusicTrack(Song song) : IMusicTrack
        {
            public Song Song { get; } = song;

            public TimeSpan Duration => Song.Duration;
        }

        private readonly ContentManager _contentManager = contentManager;

        public ISoundEffect LoadSound(string contentPath)
        {
            return new XnaSoundEffect(_contentManager.Load<SoundEffect>(contentPath));
        }

        public IMusicTrack LoadMusic(string contentPath)
        {
            return new XnaMusicTrack(_contentManager.Load<Song>(contentPath));
        }

        public void PlayMusic(IMusicTrack track, bool repeating)
        {
            Song song = ((XnaMusicTrack)track).Song;
            XnaMediaPlayer.IsRepeating = repeating;
            XnaMediaPlayer.Play(song);
        }

        public void StopMusic()
        {
            XnaMediaPlayer.Stop();
        }

        public void PauseMusic()
        {
            XnaMediaPlayer.Pause();
        }

        public void ResumeMusic()
        {
            XnaMediaPlayer.Resume();
        }

        public AudioPlaybackState MusicState => XnaMediaPlayer.State switch
        {
            MediaState.Playing => AudioPlaybackState.Playing,
            MediaState.Paused => AudioPlaybackState.Paused,
            MediaState.Stopped => AudioPlaybackState.Stopped,
            _ => AudioPlaybackState.Stopped,
        };

        public bool TryInstallSongCompletionCallback(IMusicTrack track, EventHandler<EventArgs> onDecoderFinished)
        {
            XnaMusicTrack xnaTrack = (XnaMusicTrack)track;
            return MonoGameSongCompletionWorkaround.TryInstall(
                xnaTrack.Song,
                (sender, args) => onDecoderFinished(xnaTrack, args));
        }
    }
}
