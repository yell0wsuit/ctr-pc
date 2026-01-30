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
    /// <summary>
    /// Video player implementation using LibVLC for cross-platform video playback.
    /// </summary>
    /// <remarks>
    /// This implementation uses LibVLCSharp to decode video frames and render them
    /// to a MonoGame texture. VLC initialization is performed asynchronously in
    /// the background to avoid blocking the main thread during startup.
    /// </remarks>
    internal sealed partial class VideoPlayerVLC : IVideoPlayer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPlayerVLC"/> class.
        /// </summary>
        /// <remarks>
        /// VLC initialization is started in a background task to prevent freezing
        /// the game during library loading.
        /// </remarks>
        public VideoPlayerVLC()
        {
            // Start VLC initialization in background to avoid freezing the game
            vlcInitTask = Task.Run(InitializeVlc);
        }

        /// <summary>
        /// Gets a value indicating whether playback is currently paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Occurs when video playback has finished or was skipped.
        /// </summary>
        public event Action PlaybackFinished;

        /// <summary>
        /// Prepares a video for playback from the specified path.
        /// </summary>
        /// <param name="moviePath">The relative path to the video file without extension.</param>
        /// <param name="mute">If <c>true</c>, audio will be muted during playback.</param>
        /// <remarks>
        /// This method prepares the video but does not start playback. Call <see cref="Start"/>
        /// to begin playing. If VLC is not ready or the file doesn't exist, the
        /// <see cref="PlaybackFinished"/> event is raised immediately.
        /// </remarks>
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
            string relativeVideoPath = ContentPaths.GetVideoPath(moviePath);
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

        /// <summary>
        /// Gets the current video frame as a texture.
        /// </summary>
        /// <returns>
        /// A <see cref="Texture2D"/> containing the current video frame, or <c>null</c>
        /// if no video is playing or playback has finished.
        /// </returns>
        /// <remarks>
        /// This method updates the texture with new frame data when available.
        /// The texture is created lazily when the first frame is decoded.
        /// </remarks>
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

        /// <summary>
        /// Determines whether a video is currently loaded and potentially playing.
        /// </summary>
        /// <returns>
        /// <c>true</c> if a media player instance exists; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPlaying()
        {
            return mediaPlayer != null;
        }

        /// <summary>
        /// Determines whether the video texture is ready for rendering.
        /// </summary>
        /// <returns>
        /// <c>true</c> if at least one frame has been decoded or if 500ms have elapsed
        /// since playback started; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The 500ms timeout prevents long black screen delays when the video
        /// takes time to start decoding.
        /// </remarks>
        public bool IsTextureReady()
        {
            if (frameCount > 0)
            {
                return true;
            }

            // Timeout after 500ms to avoid long black screen delay
            return playStartTime.HasValue && (DateTime.UtcNow - playStartTime.Value).TotalMilliseconds > 500;
        }

        /// <summary>
        /// Stops the current video playback.
        /// </summary>
        public void Stop()
        {
            if (mediaPlayer == null)
            {
                return;
            }

            mediaPlayer.Stop();
            playbackFinished = true;
        }

        /// <summary>
        /// Pauses the current video playback.
        /// </summary>
        public void Pause()
        {
            if (!IsPaused)
            {
                IsPaused = true;
                mediaPlayer?.SetPause(true);
            }
        }

        /// <summary>
        /// Resumes video playback after being paused.
        /// </summary>
        public void Resume()
        {
            if (IsPaused)
            {
                IsPaused = false;
                mediaPlayer?.SetPause(false);
            }
        }

        /// <summary>
        /// Starts video playback after a video has been prepared with <see cref="Play"/>.
        /// </summary>
        public void Start()
        {
            if (waitForStart && mediaPlayer != null && !mediaPlayer.IsPlaying)
            {
                waitForStart = false;
                playStartTime = DateTime.UtcNow;
                _ = mediaPlayer.Play();
            }
        }

        /// <summary>
        /// Updates the video player state and raises the <see cref="PlaybackFinished"/>
        /// event when playback completes.
        /// </summary>
        /// <remarks>
        /// This method should be called each frame to handle playback completion events.
        /// </remarks>
        public void Update()
        {
            if (!waitForStart && mediaPlayer != null && playbackFinished)
            {
                Cleanup();
                IsPaused = false;
                PlaybackFinished?.Invoke();
            }
        }

        /// <summary>
        /// Releases all resources used by the video player.
        /// </summary>
        public void Dispose()
        {
            Cleanup();
            libVlc?.Dispose();
            libVlc = null;
        }

        /// <summary>
        /// Checks if VLC is initialized and ready for use.
        /// </summary>
        /// <returns><c>true</c> if VLC initialization completed successfully; otherwise, <c>false</c>.</returns>
        private bool EnsureVlc()
        {
            // Check if background initialization is complete without blocking
            return vlcInitTask != null && vlcInitTask.IsCompleted && !vlcInitFailed && libVlc != null;
        }

        /// <summary>
        /// Initializes the LibVLC library.
        /// </summary>
        /// <remarks>
        /// On Linux, XInitThreads is called first to enable X11 multithreading support.
        /// </remarks>
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

        /// <summary>
        /// Callback invoked by VLC to negotiate the video format.
        /// </summary>
        /// <param name="opaque">User-defined opaque pointer.</param>
        /// <param name="chroma">Pointer to the chroma format string to be set.</param>
        /// <param name="width">Video width in pixels.</param>
        /// <param name="height">Video height in pixels.</param>
        /// <param name="pitches">Output pitch (bytes per row) for the video buffer.</param>
        /// <param name="lines">Output number of lines (rows) in the video buffer.</param>
        /// <returns>The number of picture buffers requested (1).</returns>
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

        /// <summary>
        /// Callback invoked by VLC when cleaning up the video format.
        /// </summary>
        /// <param name="opaque">User-defined opaque pointer.</param>
        private void CleanupVideoFormatCallback(ref IntPtr opaque)
        {
        }

        /// <summary>
        /// Callback invoked by VLC to lock the video buffer for writing.
        /// </summary>
        /// <param name="opaque">User-defined opaque pointer.</param>
        /// <param name="planes">Pointer to receive the address of the video buffer planes.</param>
        /// <returns>A picture identifier (unused, returns <see cref="IntPtr.Zero"/>).</returns>
        private IntPtr LockVideoCallback(IntPtr opaque, IntPtr planes)
        {
            if (videoBufferHandle.IsAllocated)
            {
                Marshal.WriteIntPtr(planes, videoBufferHandle.AddrOfPinnedObject());
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Callback invoked by VLC to unlock the video buffer after writing.
        /// </summary>
        /// <param name="opaque">User-defined opaque pointer.</param>
        /// <param name="picture">Picture identifier returned by <see cref="LockVideoCallback"/>.</param>
        /// <param name="planes">Pointer to the video buffer planes.</param>
        private void UnlockVideoCallback(IntPtr opaque, IntPtr picture, IntPtr planes)
        {
        }

        /// <summary>
        /// Callback invoked by VLC when a frame is ready to be displayed.
        /// </summary>
        /// <param name="opaque">User-defined opaque pointer.</param>
        /// <param name="picture">Picture identifier returned by <see cref="LockVideoCallback"/>.</param>
        private void DisplayVideoCallback(IntPtr opaque, IntPtr picture)
        {
            lock (bufferLock)
            {
                frameCount++;
                frameReady = true;
            }
        }

        /// <summary>
        /// Handles the VLC media player's EndReached event.
        /// </summary>
        /// <param name="sender">The event source.</param>
        /// <param name="args">The event arguments.</param>
        private void OnEndReached(object sender, EventArgs args)
        {
            playbackFinished = true;
        }

        /// <summary>
        /// Creates the video texture with the decoded frame dimensions.
        /// </summary>
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

        /// <summary>
        /// Releases all resources associated with the current video playback session.
        /// </summary>
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

        /// <summary>Lock object for thread-safe access to the video buffer.</summary>
        private readonly Lock bufferLock = new();

        /// <summary>Background task for VLC initialization.</summary>
        private readonly Task vlcInitTask;

        /// <summary>The LibVLC instance used for video decoding.</summary>
        private LibVLC libVlc;

        /// <summary>Indicates whether VLC initialization failed.</summary>
        private bool vlcInitFailed;

        /// <summary>The current VLC media being played.</summary>
        private VlcMedia media;

        /// <summary>The VLC media player instance.</summary>
        private VlcMediaPlayer mediaPlayer;

        /// <summary>The texture containing the current video frame.</summary>
        private Texture2D videoTexture;

        /// <summary>Buffer holding the raw video frame data in RGBA format.</summary>
        private byte[] videoBuffer;

        /// <summary>GC handle pinning the video buffer in memory for VLC access.</summary>
        private GCHandle videoBufferHandle;

        /// <summary>Indicates that a new texture needs to be created.</summary>
        private bool pendingTextureInit;

        /// <summary>Indicates that a new frame is ready to be copied to the texture.</summary>
        private bool frameReady;

        /// <summary>Indicates that playback has finished.</summary>
        private volatile bool playbackFinished;

        /// <summary>Width of the video in pixels.</summary>
        private int videoWidth;

        /// <summary>Height of the video in pixels.</summary>
        private int videoHeight;

        /// <summary>Number of frames decoded so far.</summary>
        private int frameCount;

        /// <summary>Timestamp when playback started, used for timeout detection.</summary>
        private DateTime? playStartTime;

        /// <summary>Indicates that playback is prepared but waiting for Start() to be called.</summary>
        private bool waitForStart;
    }
}
#endif
