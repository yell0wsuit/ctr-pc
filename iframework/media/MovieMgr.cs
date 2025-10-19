using System;
using CutTheRope.ios;
using CutTheRope.windows;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace CutTheRope.iframework.media
{
	// Token: 0x0200005A RID: 90
	internal class MovieMgr : NSObject
	{
		// Token: 0x0600030C RID: 780 RVA: 0x000122E4 File Offset: 0x000104E4
		public void playURL(NSString moviePath, bool mute)
		{
			this.url = moviePath;
			if (Global.ScreenSizeManager.CurrentSize.Width <= 1024)
			{
				this.video = Global.XnaGame.Content.Load<Video>("video/" + ((moviePath != null) ? moviePath.ToString() : null));
			}
			else
			{
				this.video = Global.XnaGame.Content.Load<Video>("video_hd/" + ((moviePath != null) ? moviePath.ToString() : null));
			}
			this.player = new VideoPlayer();
			this.player.IsLooped = false;
			this.player.IsMuted = mute;
			this.waitForStart = true;
		}

		// Token: 0x0600030D RID: 781 RVA: 0x00012392 File Offset: 0x00010592
		public Texture2D getTexture()
		{
			if (this.player != null && this.player.State != MediaState.Stopped)
			{
				return this.player.GetTexture();
			}
			return null;
		}

		// Token: 0x0600030E RID: 782 RVA: 0x000123B6 File Offset: 0x000105B6
		public bool isPlaying()
		{
			return this.player != null;
		}

		// Token: 0x0600030F RID: 783 RVA: 0x000123C1 File Offset: 0x000105C1
		public void stop()
		{
			if (this.player != null)
			{
				this.player.Stop();
			}
		}

		// Token: 0x06000310 RID: 784 RVA: 0x000123D6 File Offset: 0x000105D6
		public void pause()
		{
			if (!this.paused)
			{
				this.paused = true;
				if (this.player != null)
				{
					this.player.Pause();
				}
			}
		}

		// Token: 0x06000311 RID: 785 RVA: 0x000123FA File Offset: 0x000105FA
		public bool isPaused()
		{
			return this.paused;
		}

		// Token: 0x06000312 RID: 786 RVA: 0x00012402 File Offset: 0x00010602
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

		// Token: 0x06000313 RID: 787 RVA: 0x00012434 File Offset: 0x00010634
		public void start()
		{
			if (this.waitForStart && this.player != null && this.player.State == MediaState.Stopped)
			{
				this.waitForStart = false;
			}
		}

		// Token: 0x06000314 RID: 788 RVA: 0x0001245C File Offset: 0x0001065C
		public void update()
		{
			if (!this.waitForStart && this.player != null && this.player.State == MediaState.Stopped)
			{
				this.player.Dispose();
				this.player = null;
				this.video = null;
				this.paused = false;
				if (this.delegateMovieMgrDelegate != null)
				{
					this.delegateMovieMgrDelegate.moviePlaybackFinished(this.url);
				}
			}
		}

		// Token: 0x04000261 RID: 609
		private VideoPlayer player;

		// Token: 0x04000262 RID: 610
		public NSString url;

		// Token: 0x04000263 RID: 611
		public MovieMgrDelegate delegateMovieMgrDelegate;

		// Token: 0x04000264 RID: 612
		private Video video;

		// Token: 0x04000265 RID: 613
		private bool waitForStart;

		// Token: 0x04000266 RID: 614
		private bool paused;
	}
}
