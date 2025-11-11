using CutTheRope.desktop;
using CutTheRope.ios;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace CutTheRope.iframework.media
{
    internal sealed class MovieMgr : NSObject, System.IDisposable
    {
        public void PlayURL(NSString moviePath, bool mute)
        {
            url = moviePath;
            video = Global.ScreenSizeManager.CurrentSize.Width <= 1024
                ? Global.XnaGame.Content.Load<Video>("video/" + (moviePath?.ToString()))
                : Global.XnaGame.Content.Load<Video>("video_hd/" + (moviePath?.ToString()));
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
                player?.Pause();
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
                if (player != null && player.State == MediaState.Paused)
                {
                    player.Resume();
                }
            }
        }

        public void Start()
        {
            if (waitForStart && player != null && player.State == MediaState.Stopped)
            {
                waitForStart = false;
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

        public NSString url;

        public IMovieMgrDelegate delegateMovieMgrDelegate;

        private Video video;

        private bool waitForStart;

        private bool paused;

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
