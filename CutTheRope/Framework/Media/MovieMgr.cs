using CutTheRope.Desktop;
using CutTheRope.Helpers;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace CutTheRope.Framework.Media
{
    internal sealed class MovieMgr : FrameworkTypes, System.IDisposable
    {
        public void PlayURL(string moviePath, bool mute)
        {
            url = moviePath;

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
        }

        public Texture2D GetTexture()
        {
            return player != null && player.State != MediaState.Stopped ? player.GetTexture() : null;
        }

        public bool IsPlaying()
        {
            return player != null;
        }

        public void Stop()
        {
            player?.Stop();
        }

        public void Pause()
        {
            if (!paused)
            {
                paused = true;
                if (player != null)
                {
                    player.IsMuted = true;
                }
            }
        }

        public bool IsPaused()
        {
            return paused;
        }

        public void Resume()
        {
            if (paused)
            {
                paused = false;
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
                paused = false;
                delegateMovieMgrDelegate?.MoviePlaybackFinished(url);
            }
        }

        private VideoPlayer player;

        public string url;

        public IMovieMgrDelegate delegateMovieMgrDelegate;

        private Video video;

        private bool waitForStart;

        private bool paused;

        public new void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
