using System;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Manages video playback and provides a unified interface for movie operations.
    /// </summary>
    /// <remarks>
    /// This class wraps platform-specific video player implementations (VLC or MonoGame)
    /// and notifies delegates when playback finishes.
    /// </remarks>
    internal sealed class MovieMgr : FrameworkTypes, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MovieMgr"/> class.
        /// </summary>
        /// <remarks>
        /// Creates a platform-specific video player (VLC for DesktopGL, MonoGame otherwise).
        /// </remarks>
        public MovieMgr()
        {
            bool hasAvFoundation =
#if MACOS_AVFOUNDATION
                true;
#else
                false;
#endif

            bool hasFfmpeg =
#if MACOS_FFMPEG
                true;
#else
                false;
#endif

            bool hasVlc =
#if DESKTOPGL_VLC
                true;
#else
                false;
#endif

            VideoPlayerBackend backend = VideoPlayerBackendSelector.Select(
                isMac: OperatingSystem.IsMacOS(),
                isMac26OrLater: OperatingSystem.IsMacOSVersionAtLeast(26),
                hasAvFoundation: hasAvFoundation,
                hasFfmpeg: hasFfmpeg,
                hasVlc: hasVlc
            );

#pragma warning disable IDE0010, IDE0066
            switch (backend)
            {
#if MACOS_AVFOUNDATION
                case VideoPlayerBackend.AVFoundation:
                    videoPlayer = new VideoPlayerAVFoundation();
                    break;
#endif
#if MACOS_FFMPEG
                case VideoPlayerBackend.Ffmpeg:
                    videoPlayer = new VideoPlayerFFmpeg();
                    break;
#endif
#if DESKTOPGL_VLC
                case VideoPlayerBackend.Vlc:
                    videoPlayer = new VideoPlayerVLC();
                    break;
#endif
                default:
                    videoPlayer = new VideoPlayerMonoGame();
                    break;
            }
#pragma warning restore IDE0010, IDE0066
            videoPlayer.PlaybackFinished += OnPlaybackFinished;
        }

        /// <summary>
        /// Prepares and initiates video playback from the specified path.
        /// </summary>
        /// <param name="moviePath">The relative path to the video file without extension.</param>
        /// <param name="mute">If <c>true</c>, audio will be muted during playback.</param>
        public void PlayURL(string moviePath, bool mute)
        {
            url = moviePath;
            videoPlayer.Play(moviePath, mute);
        }

        /// <summary>
        /// Gets the current video frame as a texture.
        /// </summary>
        /// <returns>
        /// A <see cref="Texture2D"/> containing the current video frame, or <c>null</c>
        /// if no video is playing or playback has finished.
        /// </returns>
        public Texture2D GetTexture()
        {
            return videoPlayer.GetTexture();
        }

        /// <summary>
        /// Determines whether a video is currently loaded and potentially playing.
        /// </summary>
        /// <returns><c>true</c> if a video is active; otherwise, <c>false</c>.</returns>
        public bool IsPlaying()
        {
            return videoPlayer.IsPlaying();
        }

        /// <summary>
        /// Determines whether the video texture is ready for rendering.
        /// </summary>
        /// <returns><c>true</c> if the texture can be rendered; otherwise, <c>false</c>.</returns>
        public bool IsTextureReady()
        {
            return videoPlayer.IsTextureReady();
        }

        /// <summary>
        /// Stops the current video playback.
        /// </summary>
        public void Stop()
        {
            if (!videoPlayer.IsPlaying())
            {
                return;
            }
            videoPlayer.Stop();
        }

        /// <summary>
        /// Pauses the current video playback.
        /// </summary>
        public void Pause()
        {
            videoPlayer.Pause();
        }

        /// <summary>
        /// Determines whether playback is currently paused.
        /// </summary>
        /// <returns><c>true</c> if playback is paused; otherwise, <c>false</c>.</returns>
        public bool IsPaused()
        {
            return videoPlayer.IsPaused;
        }

        /// <summary>
        /// Resumes video playback after being paused.
        /// </summary>
        public void Resume()
        {
            videoPlayer.Resume();
        }

        /// <summary>
        /// Starts video playback after a video has been prepared with <see cref="PlayURL"/>.
        /// </summary>
        public void Start()
        {
            videoPlayer.Start();
        }

        /// <summary>
        /// Updates the video player state each frame.
        /// </summary>
        public void Update()
        {
            videoPlayer.Update();
        }

        /// <summary>
        /// Handles the video player's playback finished event and notifies the delegate.
        /// </summary>
        private void OnPlaybackFinished()
        {
            delegateMovieMgrDelegate?.MoviePlaybackFinished(url);
        }

        /// <summary>
        /// Releases all resources used by the movie manager.
        /// </summary>
        public new void Dispose()
        {
            videoPlayer.PlaybackFinished -= OnPlaybackFinished;
            videoPlayer.Dispose();
        }

#pragma warning disable CA1859
        /// <summary>The underlying video player implementation.</summary>
        private readonly IVideoPlayer videoPlayer;
#pragma warning restore CA1859

        /// <summary>The URL or path of the currently playing video.</summary>
        public string url;

        /// <summary>Delegate to notify when movie playback events occur.</summary>
        public IMovieMgrDelegate delegateMovieMgrDelegate;
    }
}
