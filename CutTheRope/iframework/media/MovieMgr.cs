using CutTheRope.desktop;
using CutTheRope.ios;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace CutTheRope.iframework.media
{
    internal class MovieMgr : NSObject
    {
        public void playURL(NSString moviePath, bool mute)
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

        public Texture2D getTexture()
        {
            return player != null && player.State != MediaState.Stopped ? player.GetTexture() : null;
        }

        public bool isPlaying()
        {
            return player != null;
        }

        public void stop()
        {
            player?.Stop();
        }

        public void pause()
        {
            if (!paused)
            {
                paused = true;
                player?.Pause();
            }
        }

        public bool isPaused()
        {
            return paused;
        }

        public void resume()
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

        public void start()
        {
            if (waitForStart && player != null && player.State == MediaState.Stopped)
            {
                waitForStart = false;
            }
        }

        public void update()
        {
            if (!waitForStart && player != null && player.State == MediaState.Stopped)
            {
                player.Dispose();
                player = null;
                video = null;
                paused = false;
                delegateMovieMgrDelegate?.moviePlaybackFinished(url);
            }
        }

        private VideoPlayer player;

        public NSString url;

        public MovieMgrDelegate delegateMovieMgrDelegate;

        private Video video;

        private bool waitForStart;

        private bool paused;
    }
}
