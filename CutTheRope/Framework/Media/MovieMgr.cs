#if MONOGAME_WINDOWSDX
using CutTheRope.Desktop;
using CutTheRope.Helpers;
#endif

#if DESKTOPGL_VLC
using CutTheRope.Desktop;
using CutTheRope.Helpers;

using LibVLCSharp.Shared;

using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using VlcMedia = LibVLCSharp.Shared.Media;
using VlcMediaPlayer = LibVLCSharp.Shared.MediaPlayer;

using System.Threading;
#endif

using System;
using System.Diagnostics;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace CutTheRope.Framework.Media
{
    internal sealed class MovieMgr : FrameworkTypes, IDisposable
    {
        public void PlayURL(string moviePath, bool mute)
        {
            url = moviePath;

#if DESKTOPGL_VLC
            Debug.WriteLine($"[MovieMgr] PlayURL called: {moviePath}");
            EnsureVlc();
            if (vlcInitFailed)
            {
                Debug.WriteLine("[MovieMgr] VLC init failed, skipping video");
                delegateMovieMgrDelegate?.MoviePlaybackFinished(url);
                return;
            }

            Debug.WriteLine("[MovieMgr] VLC initialized successfully");
            CleanupVlc();
            playbackFinished = false;
            string relativeVideoPath = ContentPaths.GetVideoPath($"{moviePath}.mp4", Global.ScreenSizeManager.CurrentSize.Width);
            string fullPath = Path.Combine(AppContext.BaseDirectory, ContentPaths.RootDirectory, ContentPaths.GetRelativePathWithContentFolder(relativeVideoPath));
            Debug.WriteLine($"[MovieMgr] Video path: {fullPath}");
            Debug.WriteLine($"[MovieMgr] File exists: {File.Exists(fullPath)}");
            if (!File.Exists(fullPath))
            {
                Debug.WriteLine("[MovieMgr] Video file not found, skipping");
                delegateMovieMgrDelegate?.MoviePlaybackFinished(url);
                return;
            }

            media = new VlcMedia(libVlc, new Uri(fullPath));
            mediaPlayer = new VlcMediaPlayer(media);
            mediaPlayer.SetVideoFormatCallbacks(VideoFormatCallback, CleanupVideoFormatCallback);
            mediaPlayer.SetVideoCallbacks(LockVideoCallback, UnlockVideoCallback, DisplayVideoCallback);
            mediaPlayer.EndReached += OnEndReached;
            mediaPlayer.Mute = mute;
            waitForStart = true;
            Debug.WriteLine("[MovieMgr] Media player created, waiting for Start()");
#elif MONOGAME_DESKTOPGL
            // Video playback not supported on DesktopGL - skip immediately
            delegateMovieMgrDelegate?.MoviePlaybackFinished(url);
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
#if DESKTOPGL_VLC
            if (mediaPlayer == null || playbackFinished)
            {
                return null;
            }

            if (pendingTextureInit)
            {
                InitializeTexture();
            }

            if (videoTexture != null && videoBuffer != null)
            {
                lock (bufferLock)
                {
                    if (frameReady)
                    {
                        textureUpdateCount++;
                        if (textureUpdateCount <= 5 || textureUpdateCount % 100 == 0)
                        {
                            // Check RGBA values of first pixel
                            byte r = videoBuffer[0];
                            byte g = videoBuffer[1];
                            byte b = videoBuffer[2];
                            byte a = videoBuffer[3];
                            Debug.WriteLine($"[MovieMgr] Uploading frame {textureUpdateCount} - first pixel RGBA: ({r}, {g}, {b}, {a})");
                        }

                        frameReady = false;
                        videoTexture.SetData(videoBuffer);
                    }
                }
            }

            return videoTexture;
#else
            return player != null && player.State != MediaState.Stopped ? player.GetTexture() : null;
#endif
        }

        public bool IsPlaying()
        {
#if DESKTOPGL_VLC
            // Return true while mediaPlayer exists so Update() can be called for cleanup
            return mediaPlayer != null;
#else
            return player != null;
#endif
        }

        public bool IsTextureReady()
        {
#if DESKTOPGL_VLC
            // Check if VLC has delivered at least one frame
            return frameCount > 0;
#else
            return player != null && player.State == MediaState.Playing;
#endif
        }

        public void Stop()
        {
#if DESKTOPGL_VLC
            if (mediaPlayer == null)
            {
                return;
            }

            mediaPlayer.Stop();
            playbackFinished = true;
#else
            player?.Stop();
#endif
        }

        public void Pause()
        {
            if (!paused)
            {
                paused = true;
#if DESKTOPGL_VLC
                mediaPlayer?.SetPause(true);
#else
                if (player != null)
                {
                    player.IsMuted = true;
                }
#endif
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
#if DESKTOPGL_VLC
                mediaPlayer?.SetPause(false);
#else
                if (player != null)
                {
                    player.IsMuted = false;
                }
#endif
            }
        }

        public void Start()
        {
#if DESKTOPGL_VLC
            if (waitForStart && mediaPlayer != null && !mediaPlayer.IsPlaying)
            {
                waitForStart = false;
                Debug.WriteLine("[MovieMgr] Starting playback...");
                bool playResult = mediaPlayer.Play();
                Debug.WriteLine($"[MovieMgr] Play() returned: {playResult}");
            }
#else
            if (waitForStart && player != null && player.State == MediaState.Stopped)
            {
                waitForStart = false;
                player.Play(video);
            }
#endif
        }

        public void Update()
        {
#if DESKTOPGL_VLC
            if (!waitForStart && mediaPlayer != null && playbackFinished)
            {
                Debug.WriteLine("[MovieMgr] Playback finished, cleaning up...");
                CleanupVlc();
                paused = false;
                Debug.WriteLine("[MovieMgr] Notifying delegate");
                delegateMovieMgrDelegate?.MoviePlaybackFinished(url);
            }
#else
            if (!waitForStart && player != null && player.State == MediaState.Stopped)
            {
                player.Dispose();
                player = null;
                video = null;
                paused = false;
                delegateMovieMgrDelegate?.MoviePlaybackFinished(url);
            }
#endif
        }

#if DESKTOPGL_VLC
        private void EnsureVlc()
        {
            if (libVlc != null || vlcInitFailed)
            {
                return;
            }

            try
            {
                Debug.WriteLine("[MovieMgr] Initializing LibVLC...");
                LibVLCSharp.Shared.Core.Initialize();
                libVlc = new LibVLC("--verbose=2");
                Debug.WriteLine("[MovieMgr] LibVLC initialized");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MovieMgr] LibVLC init failed: {ex.Message}");
                vlcInitFailed = true;
            }
        }

        private uint VideoFormatCallback(ref IntPtr opaque, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines)
        {
            Debug.WriteLine($"[MovieMgr] VideoFormatCallback: {width}x{height}");
            // Use RGBA format to match MonoGame's SurfaceFormat.Color
            const string chromaCode = "RGBA";
            byte[] chromaBytes = Encoding.ASCII.GetBytes(chromaCode);
            Marshal.Copy(chromaBytes, 0, chroma, chromaBytes.Length);

            videoWidth = (int)width;
            videoHeight = (int)height;
            pitches = width * 4;
            lines = height;

            if (videoBufferHandle.IsAllocated)
            {
                videoBufferHandle.Free();
            }

            int bufferSize = checked(videoWidth * videoHeight * 4);
            videoBuffer = new byte[bufferSize];
            videoBufferHandle = GCHandle.Alloc(videoBuffer, GCHandleType.Pinned);
            pendingTextureInit = true;
            frameReady = false;

            Debug.WriteLine($"[MovieMgr] Buffer allocated: {bufferSize} bytes");
            return 1;
        }

        private void CleanupVideoFormatCallback(ref IntPtr opaque)
        {
        }

        private IntPtr LockVideoCallback(IntPtr opaque, IntPtr planes)
        {
            if (videoBufferHandle.IsAllocated)
            {
                Marshal.WriteIntPtr(planes, videoBufferHandle.AddrOfPinnedObject());
            }

            return IntPtr.Zero;
        }

        private void UnlockVideoCallback(IntPtr opaque, IntPtr picture, IntPtr planes)
        {
        }

        private void DisplayVideoCallback(IntPtr opaque, IntPtr picture)
        {
            lock (bufferLock)
            {
                frameCount++;
                if (frameCount <= 5 || frameCount % 100 == 0)
                {
                    Debug.WriteLine($"[MovieMgr] Frame {frameCount} ready");
                }

                frameReady = true;
            }
        }

        private int frameCount;

        private int textureUpdateCount;

        private void OnEndReached(object sender, EventArgs args)
        {
            Debug.WriteLine($"[MovieMgr] Playback ended after {frameCount} frames");
            playbackFinished = true;
        }

        private void InitializeTexture()
        {
            Debug.WriteLine($"[MovieMgr] InitializeTexture: {videoWidth}x{videoHeight}");
            pendingTextureInit = false;
            if (videoWidth <= 0 || videoHeight <= 0)
            {
                Debug.WriteLine("[MovieMgr] Invalid dimensions, skipping texture init");
                return;
            }

            videoTexture?.Dispose();
            videoTexture = new Texture2D(Global.GraphicsDevice, videoWidth, videoHeight, false, SurfaceFormat.Color);
            Debug.WriteLine("[MovieMgr] Texture created");
        }

        private void CleanupVlc()
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.EndReached -= OnEndReached;
                if (mediaPlayer.IsPlaying)
                {
                    mediaPlayer.Stop();
                }

                mediaPlayer.Dispose();
                mediaPlayer = null;
            }

            media?.Dispose();
            media = null;

            if (videoBufferHandle.IsAllocated)
            {
                videoBufferHandle.Free();
            }

            videoTexture?.Dispose();
            videoTexture = null;
            videoBuffer = null;
            pendingTextureInit = false;
            frameReady = false;
            playbackFinished = false;
            frameCount = 0;
            textureUpdateCount = 0;
            videoWidth = 0;
            videoHeight = 0;
        }
#endif

#if DESKTOPGL_VLC
        private readonly Lock bufferLock = new();

        private LibVLC libVlc;

        private bool vlcInitFailed;

        private VlcMedia media;

        private VlcMediaPlayer mediaPlayer;

        private Texture2D videoTexture;

        private byte[] videoBuffer;

        private GCHandle videoBufferHandle;

        private bool pendingTextureInit;

        private bool frameReady;

        private volatile bool playbackFinished;

        private int videoWidth;

        private int videoHeight;
#endif

#if !DESKTOPGL_VLC
        private VideoPlayer player;

        private Video video;
#endif

        private bool waitForStart;

        private bool paused;

        public string url;

        public IMovieMgrDelegate delegateMovieMgrDelegate;

        public new void Dispose()
        {
#if DESKTOPGL_VLC
            CleanupVlc();
            libVlc?.Dispose();
            libVlc = null;
#else
            player?.Dispose();
            player = null;
            video = null;
#endif
        }
    }
}
