#if MACOS_AVFOUNDATION
using System;
using System.IO;

using AVFoundation;

using CoreMedia;

using CoreVideo;

using CutTheRope.Desktop;
using CutTheRope.Helpers;

using Foundation;

using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Video player implementation using macOS AVFoundation framework.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="AVPlayer"/> for playback and <see cref="AVPlayerItemVideoOutput"/>
    /// for extracting video frames as pixel buffers, which are then converted to
    /// MonoGame textures. Requires macOS 26 or later.
    /// </remarks>
    internal sealed class VideoPlayerAVFoundation : IVideoPlayer
    {
        /// <inheritdoc/>
        public bool IsPaused { get; private set; }

        /// <inheritdoc/>
        public event Action PlaybackFinished;

        /// <inheritdoc/>
        public void Play(string moviePath, bool mute)
        {
            Console.WriteLine($"[AVFoundation] Play requested: {moviePath}, mute={mute}");

            Cleanup();
            playbackFinished = false;
            frameCount = 0;
            playStartTime = null;
            loggedFirstFrame = false;

            string basePath = NSBundle.MainBundle.ResourcePath;
            string relativeVideoPath = ContentPaths.GetVideoPath($"{moviePath}");
            string fullPath = Path.Combine(
                basePath,
                ContentPaths.RootDirectory,
                ContentPaths.GetRelativePathWithContentFolder(relativeVideoPath)
            );

            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"[AVFoundation] Missing video: {fullPath}");
                PlaybackFinished?.Invoke();
                return;
            }

            NSUrl url = NSUrl.FromFilename(fullPath);
            playerItem = new AVPlayerItem(url);

            CVPixelBufferAttributes pixelAttributes = new()
            {
                PixelFormatType = CVPixelFormatType.CV32BGRA
            };

            videoOutput = new AVPlayerItemVideoOutput(pixelAttributes)
            {
                SuppressesPlayerRendering = true
            };
            playerItem.AddOutput(videoOutput);

            player = new AVPlayer(playerItem)
            {
                Muted = mute
            };

            playbackObserver = NSNotificationCenter.DefaultCenter.AddObserver(
                AVPlayerItem.DidPlayToEndTimeNotification,
                notification =>
                {
                    if (notification.Object == playerItem)
                    {
                        OnPlaybackFinished();
                    }
                });

            waitForStart = true;
        }

        /// <inheritdoc/>
        public Texture2D GetTexture()
        {
            if (player == null || videoOutput == null || playbackFinished)
            {
                Console.WriteLine($"[AVFoundation] GetTexture early return: player={player != null}, videoOutput={videoOutput != null}, playbackFinished={playbackFinished}, videoTexture={videoTexture != null}");
                return videoTexture;
            }

            CMTime itemTime = player.CurrentTime;
            if (!videoOutput.HasNewPixelBufferForItemTime(itemTime))
            {
                return videoTexture;
            }

            CMTime displayTime = default;
            using CVPixelBuffer pixelBuffer = videoOutput.CopyPixelBuffer(itemTime, ref displayTime);
            if (pixelBuffer == null)
            {
                return videoTexture;
            }

            _ = pixelBuffer.Lock(CVPixelBufferLock.ReadOnly);
            try
            {
                int width = (int)pixelBuffer.Width;
                int height = (int)pixelBuffer.Height;

                EnsureTexture(width, height);
                CopyPixelBuffer(pixelBuffer, width, height);
            }
            finally
            {
                _ = pixelBuffer.Unlock(CVPixelBufferLock.ReadOnly);
            }

            if (videoTexture != null && videoBuffer != null)
            {
                videoTexture.SetData(videoBuffer);
            }

            if (!loggedFirstFrame && videoTexture != null)
            {
                loggedFirstFrame = true;
                Console.WriteLine($"[AVFoundation] First frame: {videoWidth}x{videoHeight}");
            }

            frameCount++;
            return videoTexture;
        }

        /// <inheritdoc/>
        public bool IsPlaying()
        {
            return player != null;
        }

        /// <inheritdoc/>
        public bool IsTextureReady()
        {
            if (frameCount > 0)
            {
                return true;
            }

            if (player != null && videoOutput != null)
            {
                CMTime itemTime = player.CurrentTime;
                if (videoOutput.HasNewPixelBufferForItemTime(itemTime))
                {
                    return true;
                }
            }

            return playStartTime.HasValue && (DateTime.UtcNow - playStartTime.Value).TotalMilliseconds > 500;
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (player == null || playbackFinished)
            {
                return;
            }

            Console.WriteLine("[AVFoundation] Stop");
            playbackFinished = true;
            player.Pause();
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (player == null || playbackFinished || IsPaused)
            {
                return;
            }

            Console.WriteLine("[AVFoundation] Pause");
            IsPaused = true;
            player.Pause();
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (player == null || playbackFinished || !IsPaused)
            {
                return;
            }

            Console.WriteLine("[AVFoundation] Resume");
            IsPaused = false;
            player.Play();
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (!waitForStart || player == null)
            {
                return;
            }

            Console.WriteLine("[AVFoundation] Start");
            waitForStart = false;
            playStartTime = DateTime.UtcNow;
            player.Play();
        }

        /// <inheritdoc/>
        public void Update()
        {
            if (!waitForStart && playbackFinished)
            {
                Console.WriteLine($"[AVFoundation] Update: triggering cleanup, videoTexture={videoTexture != null}");
                Cleanup();
                IsPaused = false;
                Console.WriteLine("[AVFoundation] Update: invoking PlaybackFinished");
                PlaybackFinished?.Invoke();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Console.WriteLine("[AVFoundation] Dispose");
            Cleanup();
            IsPaused = false;
        }

        /// <summary>
        /// Ensures the video texture exists and matches the specified dimensions.
        /// </summary>
        /// <param name="width">Required texture width.</param>
        /// <param name="height">Required texture height.</param>
        private void EnsureTexture(int width, int height)
        {
            if (videoTexture != null && width == videoWidth && height == videoHeight)
            {
                return;
            }

            videoTexture?.Dispose();
            videoTexture = new Texture2D(Global.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            videoWidth = width;
            videoHeight = height;

            int bufferSize = checked(width * height * 4);
            if (videoBuffer == null || videoBuffer.Length != bufferSize)
            {
                videoBuffer = new byte[bufferSize];
            }
        }

        /// <summary>
        /// Copies pixel data from a CoreVideo buffer to the managed video buffer.
        /// </summary>
        /// <param name="pixelBuffer">The source pixel buffer from AVFoundation.</param>
        /// <param name="width">Frame width in pixels.</param>
        /// <param name="height">Frame height in pixels.</param>
        /// <remarks>
        /// Converts BGRA pixel format to RGBA for MonoGame texture compatibility.
        /// </remarks>
        private unsafe void CopyPixelBuffer(CVPixelBuffer pixelBuffer, int width, int height)
        {
            if (videoBuffer == null)
            {
                return;
            }

            IntPtr baseAddress = pixelBuffer.BaseAddress;
            if (baseAddress == IntPtr.Zero)
            {
                return;
            }

            int bytesPerRow = (int)pixelBuffer.BytesPerRow;
            int dstStride = width * 4;

            bool isBgra = pixelBuffer.PixelFormatType == CVPixelFormatType.CV32BGRA;

            byte* srcBase = (byte*)baseAddress.ToPointer();
            fixed (byte* dstBase = videoBuffer)
            {
                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = srcBase + (y * bytesPerRow);
                    byte* dstRow = dstBase + (y * dstStride);

                    if (isBgra)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int offset = x * 4;
                            dstRow[offset] = srcRow[offset + 2];
                            dstRow[offset + 1] = srcRow[offset + 1];
                            dstRow[offset + 2] = srcRow[offset];
                            dstRow[offset + 3] = srcRow[offset + 3];
                        }
                    }
                    else
                    {
                        Buffer.MemoryCopy(srcRow, dstRow, dstStride, dstStride);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the AVFoundation playback finished notification.
        /// </summary>
        private void OnPlaybackFinished()
        {
            Console.WriteLine($"[AVFoundation] Playback finished, videoTexture={videoTexture != null}");
            playbackFinished = true;
        }

        /// <summary>
        /// Releases all AVFoundation and video resources.
        /// </summary>
        private void Cleanup()
        {
            if (playbackObserver != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(playbackObserver);
                playbackObserver.Dispose();
                playbackObserver = null;
            }

            if (player != null)
            {
                player.Pause();
                player.Dispose();
                player = null;
            }

            videoOutput?.Dispose();
            videoOutput = null;

            playerItem?.Dispose();
            playerItem = null;

            videoTexture?.Dispose();
            videoTexture = null;
            videoBuffer = null;
            videoWidth = 0;
            videoHeight = 0;
            frameCount = 0;
            playStartTime = null;
            waitForStart = false;
            playbackFinished = false;
        }

        /// <summary>The AVFoundation media player instance.</summary>
        private AVPlayer player;

        /// <summary>The media item being played.</summary>
        private AVPlayerItem playerItem;

        /// <summary>Video output for extracting pixel buffers from the player.</summary>
        private AVPlayerItemVideoOutput videoOutput;

        /// <summary>Observer for playback finished notifications.</summary>
        private NSObject playbackObserver;

        /// <summary>MonoGame texture for rendering video frames.</summary>
        private Texture2D videoTexture;

        /// <summary>Managed buffer for transferring frame data to the texture.</summary>
        private byte[] videoBuffer;

        /// <summary>Current video width in pixels.</summary>
        private int videoWidth;

        /// <summary>Current video height in pixels.</summary>
        private int videoHeight;

        /// <summary>Number of frames decoded so far.</summary>
        private int frameCount;

        /// <summary>Timestamp when playback started, used for texture ready timeout.</summary>
        private DateTime? playStartTime;

        /// <summary>Indicates the player is waiting for Start() to be called.</summary>
        private bool waitForStart;

        /// <summary>Indicates playback has finished.</summary>
        private volatile bool playbackFinished;

        /// <summary>Whether the first frame has been logged for debugging.</summary>
        private bool loggedFirstFrame;
    }
}
#endif
