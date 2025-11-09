using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;

namespace CutTheRope.iframework.media
{
    internal class MovieMgr : NSObject
    {
        public void playURL(NSString moviePath, bool mute)
        {
            this.url = moviePath;
            if (Global.ScreenSizeManager.CurrentSize.Width <= 1024)
            {
                this.video = Global.XnaGame.Content.Load<Video>("video/" + (moviePath?.ToString()));
            }
            else
            {
                this.video = Global.XnaGame.Content.Load<Video>("video_hd/" + (moviePath?.ToString()));
            }
            this.player = new VideoPlayer();
            this.player.IsLooped = false;
            this.player.IsMuted = mute;
            this.waitForStart = true;
        }

        public Texture2D getTexture()
        {
            if (this.player != null && this.player.State != MediaState.Stopped)
            {
                return this.player.GetTexture();
            }
            return null;
        }

        public bool isPlaying()
        {
            return this.player != null;
        }

        public void stop()
        {
            this.player?.Stop();
        }

        public void pause()
        {
            if (!this.paused)
            {
                this.paused = true;
                this.player?.Pause();
            }
        }

        public bool isPaused()
        {
            return this.paused;
        }

        public void resume()
        {
            if (this.paused)
            {
                this.paused = false;
                if (this.player != null && this.player.State == MediaState.Paused)
                {
                    this.player.Resume();
                }
            }
        }

        public void start()
        {
            if (this.waitForStart && this.player != null && this.player.State == MediaState.Stopped)
            {
                this.waitForStart = false;
            }
        }

        public void update()
        {
            if (!this.waitForStart && this.player != null && this.player.State == MediaState.Stopped)
            {
                this.player.Dispose();
                this.player = null;
                this.video = null;
                this.paused = false;
                this.delegateMovieMgrDelegate?.moviePlaybackFinished(this.url);
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
