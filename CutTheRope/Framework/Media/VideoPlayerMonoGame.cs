#if !DESKTOPGL_VLC
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
