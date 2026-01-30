using System;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Media
{
    internal sealed class MovieMgr : FrameworkTypes, IDisposable
    {
        public MovieMgr()
        {
#if DESKTOPGL_VLC
            videoPlayer = new VideoPlayerVLC();
#else
            videoPlayer = new VideoPlayerMonoGame();
#endif
            videoPlayer.PlaybackFinished += OnPlaybackFinished;
        }

        public void PlayURL(string moviePath, bool mute)
        {
            url = moviePath;
            videoPlayer.Play(moviePath, mute);
        }

        public Texture2D GetTexture()
        {
            return videoPlayer.GetTexture();
        }

        public bool IsPlaying()
        {
            return videoPlayer.IsPlaying();
        }

        public bool IsTextureReady()
        {
            return videoPlayer.IsTextureReady();
        }

        public void Stop()
        {
            videoPlayer.Stop();
        }

        public void Pause()
        {
            videoPlayer.Pause();
        }

        public bool IsPaused()
        {
            return videoPlayer.IsPaused;
        }

        public void Resume()
        {
            videoPlayer.Resume();
        }

        public void Start()
        {
            videoPlayer.Start();
        }

        public void Update()
        {
            videoPlayer.Update();
        }

        private void OnPlaybackFinished()
        {
            delegateMovieMgrDelegate?.MoviePlaybackFinished(url);
        }

        public new void Dispose()
        {
            videoPlayer.PlaybackFinished -= OnPlaybackFinished;
            videoPlayer.Dispose();
        }

#pragma warning disable CA1859
        private readonly IVideoPlayer videoPlayer;
#pragma warning restore CA1859

        public string url;

        public IMovieMgrDelegate delegateMovieMgrDelegate;
    }
}
