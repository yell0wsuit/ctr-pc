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
        /// <summary>
        /// Wraps a MonoGame <see cref="SoundEffect"/> together with the content manager that owns it.
        /// </summary>
        /// <remarks>
        /// A loaded asset belongs to its content manager's cache, so releasing one means unloading
        /// that manager. Disposing the effect on its own would leave the dead object in the cache,
        /// and every later load of the same asset would hand that corpse back: creating an instance
        /// from it throws, and the sound is silent for the rest of the process. Each effect
        /// therefore gets its own manager, exactly as <see cref="Images"/> does for textures.
        /// </remarks>
        private sealed class XnaSoundEffect(ContentManager owner, SoundEffect effect) : ISoundEffect
        {
            public SoundEffect Effect { get; } = effect;

            public ISoundInstance CreateInstance()
            {
                return new XnaSoundInstance(Effect.CreateInstance());
            }

            public void Dispose()
            {
                owner.Unload();
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
            // Sound effects are freed one at a time when a gameplay session ends, so each gets
            // its own manager to unload. Music stays on the shared manager: it is never freed.
            DesktopContentManager owner = new(_contentManager.ServiceProvider);
            return new XnaSoundEffect(owner, owner.Load<SoundEffect>(contentPath));
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
