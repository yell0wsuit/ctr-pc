#if !DESKTOPGL_VLC
using System;

#if !MONOGAME_DESKTOPGL
using CutTheRope.Desktop;
using CutTheRope.Helpers;
#endif

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace CutTheRope.Framework.Media
{
    internal sealed class VideoPlayerMonoGame : IVideoPlayer
    {
        public bool IsPaused { get; private set; }

        public event Action PlaybackFinished;

        public void Play(string moviePath, bool mute)
        {
#if MONOGAME_DESKTOPGL
            // Video playback not supported on DesktopGL - skip immediately
            PlaybackFinished?.Invoke();
#else
            string videoPath = ContentPaths.GetVideoPath(moviePath, Global.ScreenSizeManager.CurrentSize.Width);

            // Unload the video from ContentManager's cache before reloading
            // Without this, ContentManager returns a disposed Video instance when playing
            // the same video multiple times, causing InvalidOperationException in VideoPlayer.Play()
            try
            {
                Global.XnaGame.Content.UnloadAsset(videoPath);
            }
            catch { }

            video = Global.XnaGame.Content.Load<Video>(videoPath);

            player = new VideoPlayer
            {
                IsLooped = false,
                IsMuted = mute
            };
            waitForStart = true;
#endif
        }

        public Texture2D GetTexture()
        {
            return player != null && player.State != MediaState.Stopped ? player.GetTexture() : null;
        }

        public bool IsPlaying()
        {
            return player != null;
        }

        public bool IsTextureReady()
        {
            return player != null && player.State == MediaState.Playing;
        }

        public void Stop()
        {
            player?.Stop();
        }

        public void Pause()
        {
            if (!IsPaused)
            {
                IsPaused = true;
                if (player != null)
                {
                    player.IsMuted = true;
                }
            }
        }

        public void Resume()
        {
            if (IsPaused)
            {
                IsPaused = false;
                if (player != null)
                {
                    player.IsMuted = false;
                }
            }
        }

        public void Start()
        {
            if (waitForStart && player != null && player.State == MediaState.Stopped)
            {
                waitForStart = false;
                player.Play(video);
            }
        }

        public void Update()
        {
            if (!waitForStart && player != null && player.State == MediaState.Stopped)
            {
                player.Dispose();
                player = null;
                video = null;
                IsPaused = false;
                PlaybackFinished?.Invoke();
            }
        }

        public void Dispose()
        {
            player?.Dispose();
            player = null;
            video = null;
        }

        private VideoPlayer player;

        private Video video;

        private bool waitForStart;
    }
}
#endif
