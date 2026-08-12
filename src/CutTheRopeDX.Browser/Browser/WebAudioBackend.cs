using System;
using System.Threading.Tasks;

using CutTheRopeDX.Framework.Media;

namespace CutTheRopeDX.Browser
{
    /// <summary>A decoded WebAudio buffer addressed by content path.</summary>
    /// <param name="key">Content path identifying the decoded buffer.</param>
    internal sealed class WebSoundEffect(string key) : ISoundEffect
    {
        /// <inheritdoc />
        public ISoundInstance CreateInstance()
        {
            return new WebSoundInstance(key);
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }

    /// <summary>One playing or stopped voice of a decoded buffer.</summary>
    /// <param name="key">Content path identifying the decoded buffer.</param>
    internal sealed class WebSoundInstance(string key) : ISoundInstance
    {
        private int _handle;

        /// <inheritdoc />
        public bool IsLooped { get; set; }

        /// <inheritdoc />
        public float Volume
        {
            get;
            set
            {
                field = value;
                if (_handle != 0)
                {
                    AudioInterop.SetVolume(_handle, value);
                }
            }
        } = 1f;

        /// <inheritdoc />
        public AudioPlaybackState State =>
            _handle != 0 && AudioInterop.IsPlaying(_handle)
                ? AudioPlaybackState.Playing
                : AudioPlaybackState.Stopped;

        /// <inheritdoc />
        public void Play()
        {
            Stop();
            _handle = AudioInterop.Play(key, IsLooped, Volume);
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (_handle != 0)
            {
                AudioInterop.Stop(_handle);
                _handle = 0;
            }
        }

        /// <inheritdoc />
        public void Pause()
        {
            Stop();
        }

        /// <inheritdoc />
        public void Resume()
        {
            Play();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Stop();
        }
    }

    /// <summary>A decoded music track.</summary>
    /// <param name="key">Content path identifying the decoded buffer.</param>
    internal sealed class WebMusicTrack(string key) : IMusicTrack
    {
        /// <summary>Content path identifying the decoded buffer.</summary>
        public string Key => key;

        /// <inheritdoc />
        public TimeSpan Duration => TimeSpan.FromSeconds(AudioInterop.DurationOf(key));
    }

    /// <summary>Audio backed by the browser's WebAudio API.</summary>
    /// <param name="contentBaseUrl">Root URL of the content tree, with a trailing slash.</param>
    internal sealed class WebAudioBackend(string contentBaseUrl) : IAudioBackend
    {
        private int _musicHandle;
        private bool _musicPaused;

        /// <summary>Decodes one audio file so it can be played synchronously later.</summary>
        /// <param name="contentPath">Content path without extension.</param>
        public Task<int> PreloadAsync(string contentPath)
        {
            return PreloadFileAsync($"{contentPath}.ogg");
        }

        /// <summary>Decodes one cataloged Ogg file so later loads never fetch it again.</summary>
        /// <param name="relativePath">Content-relative Ogg path including its extension.</param>
        public Task<int> PreloadFileAsync(string relativePath)
        {
            const string extension = ".ogg";
            if (!relativePath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Expected an Ogg content path.", nameof(relativePath));
            }

            string key = relativePath[..^extension.Length];
            return AudioInterop.Decode(key, contentBaseUrl + relativePath);
        }

        /// <inheritdoc />
        public ISoundEffect LoadSound(string contentPath)
        {
            _ = PreloadAsync(contentPath);
            return new WebSoundEffect(contentPath);
        }

        /// <inheritdoc />
        public IMusicTrack LoadMusic(string contentPath)
        {
            _ = PreloadAsync(contentPath);
            return new WebMusicTrack(contentPath);
        }

        /// <inheritdoc />
        public void PlayMusic(IMusicTrack track, bool repeating)
        {
            StopMusic();
            if (track is WebMusicTrack web)
            {
                _musicHandle = AudioInterop.Play(web.Key, repeating, 1f);
                _musicPaused = false;
            }
        }

        /// <inheritdoc />
        public void StopMusic()
        {
            if (_musicHandle != 0)
            {
                AudioInterop.Stop(_musicHandle);
                _musicHandle = 0;
            }
            _musicPaused = false;
        }

        /// <inheritdoc />
        public void PauseMusic()
        {
            if (_musicHandle != 0)
            {
                AudioInterop.PauseVoice(_musicHandle);
                _musicPaused = true;
            }
        }

        /// <inheritdoc />
        public void ResumeMusic()
        {
            if (_musicHandle != 0)
            {
                AudioInterop.ResumeVoice(_musicHandle);
                _musicPaused = false;
            }
        }

        /// <inheritdoc />
        public AudioPlaybackState MusicState =>
            _musicHandle == 0 || !AudioInterop.IsPlaying(_musicHandle)
                ? AudioPlaybackState.Stopped
                : _musicPaused ? AudioPlaybackState.Paused : AudioPlaybackState.Playing;

        /// <inheritdoc />
        public bool TryInstallSongCompletionCallback(
            IMusicTrack track, EventHandler<EventArgs> onDecoderFinished)
        {
            return false;
        }
    }
}
