#if !DESKTOPGL_VLC
using System;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Stub video player for platforms without VLC support (e.g., macOS).
    /// Skips video playback immediately. Will be replaced with AVFoundation on macOS.
    /// </summary>
    internal sealed class VideoPlayerMonoGame : IVideoPlayer
    {
        public bool IsPaused { get; private set; }

        public event Action PlaybackFinished;

        public void Play(string moviePath, bool mute)
        {
            // Video playback not supported - skip immediately
            PlaybackFinished?.Invoke();
        }

        public Texture2D GetTexture()
        {
            return null;
        }

        public bool IsPlaying()
        {
            return false;
        }

        public bool IsTextureReady()
        {
            return false;
        }

        public void Stop() { }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void Start() { }

        public void Update() { }

        public void Dispose() { }
    }
}
#endif
