using CutTheRope.ios;
using CutTheRope.desktop;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;

namespace CutTheRope.iframework.media
{
    internal class MovieMgr : NSObject
    {
        public void playURL(NSString moviePath, bool mute)
        {
            url = moviePath;
            if (Global.ScreenSizeManager.CurrentSize.Width <= 1024)
            {
                video = Global.XnaGame.Content.Load<Video>("video/" + (moviePath?.ToString()));
            }
            else
            {
                video = Global.XnaGame.Content.Load<Video>("video_hd/" + (moviePath?.ToString()));
            }
            player = new VideoPlayer();
            player.IsLooped = false;
            player.IsMuted = mute;
            waitForStart = true;
        }

        public Texture2D getTexture()
        {
            if (player != null && player.State != MediaState.Stopped)
            {
                return player.GetTexture();
            }
            return null;
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
