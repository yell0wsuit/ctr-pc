#if DESKTOPGL_VLC
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CutTheRope.Desktop;
using CutTheRope.Helpers;

using LibVLCSharp.Shared;

using Microsoft.Xna.Framework.Graphics;

using VlcMedia = LibVLCSharp.Shared.Media;
using VlcMediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace CutTheRope.Framework.Media
{
    internal sealed partial class VideoPlayerVLC : IVideoPlayer
    {
        public VideoPlayerVLC()
        {
            // Start VLC initialization in background to avoid freezing the game
            vlcInitTask = Task.Run(InitializeVlc);
        }

        public bool IsPaused { get; private set; }

        public event Action PlaybackFinished;

        public void Play(string moviePath, bool mute)
        {
            if (!EnsureVlc())
            {
                // VLC not ready yet or failed to initialize, skip video
                PlaybackFinished?.Invoke();
                return;
            }

            Cleanup();
            playbackFinished = false;
            playStartTime = null;
            string relativeVideoPath = ContentPaths.GetVideoPath($"{moviePath}.mp4", Global.ScreenSizeManager.CurrentSize.Width);
            string fullPath = Path.Combine(AppContext.BaseDirectory, ContentPaths.RootDirectory, ContentPaths.GetRelativePathWithContentFolder(relativeVideoPath));
            if (!File.Exists(fullPath))
            {
                PlaybackFinished?.Invoke();
                return;
            }

            media = new VlcMedia(libVlc, new Uri(fullPath));
            mediaPlayer = new VlcMediaPlayer(media);
            mediaPlayer.SetVideoFormatCallbacks(VideoFormatCallback, CleanupVideoFormatCallback);
            mediaPlayer.SetVideoCallbacks(LockVideoCallback, UnlockVideoCallback, DisplayVideoCallback);
            mediaPlayer.EndReached += OnEndReached;
            mediaPlayer.Mute = mute;
            waitForStart = true;
        }

        public Texture2D GetTexture()
        {
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
                        frameReady = false;
                        videoTexture.SetData(videoBuffer);
                    }
                }
            }

            return videoTexture;
        }

        public bool IsPlaying()
        {
            return mediaPlayer != null;
        }

        public bool IsTextureReady()
        {
            if (frameCount > 0)
            {
                return true;
            }

            // Timeout after 500ms to avoid long black screen delay
            return playStartTime.HasValue && (DateTime.UtcNow - playStartTime.Value).TotalMilliseconds > 500;
        }

        public void Stop()
        {
            if (mediaPlayer == null)
            {
                return;
            }

            mediaPlayer.Stop();
            playbackFinished = true;
        }

        public void Pause()
        {
            if (!IsPaused)
            {
                IsPaused = true;
                mediaPlayer?.SetPause(true);
            }
        }

        public void Resume()
        {
            if (IsPaused)
            {
                IsPaused = false;
                mediaPlayer?.SetPause(false);
            }
        }

        public void Start()
        {
            if (waitForStart && mediaPlayer != null && !mediaPlayer.IsPlaying)
            {
                waitForStart = false;
                playStartTime = DateTime.UtcNow;
                _ = mediaPlayer.Play();
            }
        }

        public void Update()
        {
            if (!waitForStart && mediaPlayer != null && playbackFinished)
            {
                Cleanup();
                IsPaused = false;
                PlaybackFinished?.Invoke();
            }
        }

        public void Dispose()
        {
            Cleanup();
            libVlc?.Dispose();
            libVlc = null;
        }

        private bool EnsureVlc()
        {
            // Check if background initialization is complete without blocking
            return vlcInitTask != null && vlcInitTask.IsCompleted && !vlcInitFailed && libVlc != null;
        }

        private void InitializeVlc()
        {
            if (libVlc != null || vlcInitFailed)
            {
                return;
            }

            try
            {
                // On Linux X11, XInitThreads must be called before creating LibVLC
                // to enable proper multithreading support
                if (OperatingSystem.IsLinux())
                {
                    try
                    {
                        _ = XInitThreads();
                    }
                    catch
                    {
                        // X11 may not be available (e.g., Wayland-only systems)
                    }
                }

                LibVLCSharp.Shared.Core.Initialize();
                libVlc = new LibVLC();
            }
            catch
            {
                vlcInitFailed = true;
            }
        }

        [LibraryImport("libX11.so.6", EntryPoint = "XInitThreads")]
        private static partial int XInitThreads();

        private uint VideoFormatCallback(ref IntPtr opaque, IntPtr chroma, ref uint width, ref uint height, ref uint pitches, ref uint lines)
        {
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
                frameReady = true;
            }
        }

        private void OnEndReached(object sender, EventArgs args)
        {
            playbackFinished = true;
        }

        private void InitializeTexture()
        {
            pendingTextureInit = false;
            if (videoWidth <= 0 || videoHeight <= 0)
            {
                return;
            }

            videoTexture?.Dispose();
            videoTexture = new Texture2D(Global.GraphicsDevice, videoWidth, videoHeight, false, SurfaceFormat.Color);
        }

        private void Cleanup()
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
            playStartTime = null;
            videoWidth = 0;
            videoHeight = 0;
        }

        private readonly Lock bufferLock = new();

        private readonly Task vlcInitTask;

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

        private int frameCount;

        private DateTime? playStartTime;

        private bool waitForStart;
    }
}
#endif
