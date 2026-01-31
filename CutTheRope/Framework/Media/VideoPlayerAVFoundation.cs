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
    internal sealed class VideoPlayerAVFoundation : IVideoPlayer
    {
        public bool IsPaused { get; private set; }

        public event Action PlaybackFinished;

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

        public bool IsPlaying()
        {
            return player != null;
        }

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

        public void Dispose()
        {
            Console.WriteLine("[AVFoundation] Dispose");
            Cleanup();
            IsPaused = false;
        }

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

        private void OnPlaybackFinished()
        {
            Console.WriteLine($"[AVFoundation] Playback finished, videoTexture={videoTexture != null}");
            playbackFinished = true;
        }

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

        private AVPlayer player;

        private AVPlayerItem playerItem;

        private AVPlayerItemVideoOutput videoOutput;

        private NSObject playbackObserver;

        private Texture2D videoTexture;

        private byte[] videoBuffer;

        private int videoWidth;

        private int videoHeight;

        private int frameCount;

        private DateTime? playStartTime;

        private bool waitForStart;

        private volatile bool playbackFinished;

        private bool loggedFirstFrame;
    }
}
#endif
