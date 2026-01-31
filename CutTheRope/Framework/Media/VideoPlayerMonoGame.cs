using System;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Stub video player when VLC, AVFoundation or FFmpeg is unavailable.
    /// Skips video playback immediately.
    /// </summary>
    internal sealed class VideoPlayerMonoGame : IVideoPlayer
    {
        /// <inheritdoc/>
        public bool IsPaused { get; private set; }

        /// <inheritdoc/>
        public event Action PlaybackFinished;

        /// <inheritdoc/>
        public void Play(string moviePath, bool mute)
        {
            // Video playback not supported - skip immediately
            PlaybackFinished?.Invoke();
        }

        /// <inheritdoc/>
        public Texture2D GetTexture()
        {
            return null;
        }

        /// <inheritdoc/>
        public bool IsPlaying()
        {
            return false;
        }

        /// <inheritdoc/>
        public bool IsTextureReady()
        {
            return false;
        }

        /// <inheritdoc/>
        public void Stop() { }

        /// <inheritdoc/>
        public void Pause()
        {
            IsPaused = true;
        }

        /// <inheritdoc/>
        public void Resume()
        {
            IsPaused = false;
        }

        /// <inheritdoc/>
        public void Start() { }

        /// <inheritdoc/>
        public void Update() { }

        /// <inheritdoc/>
        public void Dispose() { }
    }
}
