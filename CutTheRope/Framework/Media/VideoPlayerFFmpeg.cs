#if DESKTOPGL_FFMPEG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using CutTheRope.Desktop;
using CutTheRope.Helpers;

using FFmpeg.AutoGen;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Video player implementation using FFmpeg for decoding and playback on macOS.
    /// </summary>
    /// <remarks>
    /// This player uses FFmpeg libraries for video/audio decoding and converts frames
    /// to RGBA format for MonoGame texture rendering. Audio is played through
    /// <see cref="DynamicSoundEffectInstance"/> with resampling handled by libswresample.
    /// </remarks>
    internal sealed unsafe class VideoPlayerFFmpeg : IVideoPlayer
    {
        /// <summary>Timeout in milliseconds before considering texture ready even without frames.</summary>
        private const int TextureReadyTimeoutMs = 500;

        /// <summary>Maximum number of audio buffers to queue for playback.</summary>
        private const int MaxQueuedAudioBuffers = 8;

        /// <summary>Bytes per audio sample (16-bit audio = 2 bytes).</summary>
        private const int BytesPerSample = 2;

        /// <summary>Lock for thread-safe frame buffer access.</summary>
        private readonly Lock bufferLock = new();

        /// <summary>Queue of decoded audio buffers waiting to be submitted.</summary>
        private readonly Queue<byte[]> pendingAudioQueue = new();

        /// <summary>Stopwatch for tracking playback time and synchronization.</summary>
        private readonly Stopwatch playbackStopwatch = new();

        /// <summary>Function to check if a file exists.</summary>
        private readonly Func<string, bool> fileExists;

        /// <summary>Whether FFmpeg native libraries were found and loaded.</summary>
        private readonly bool librariesLoaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPlayerFFmpeg"/> class.
        /// </summary>
        public VideoPlayerFFmpeg()
            : this(File.Exists, baseDir => FfmpegRootPathResolver.Resolve(baseDir, Directory.Exists))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoPlayerFFmpeg"/> class with custom dependencies.
        /// </summary>
        /// <param name="fileExists">Function to check file existence.</param>
        /// <param name="resolveRootPath">Function to resolve FFmpeg library path.</param>
        internal VideoPlayerFFmpeg(Func<string, bool> fileExists, Func<string, string> resolveRootPath)
        {
            this.fileExists = fileExists;

            string ffmpegRoot = resolveRootPath(AppContext.BaseDirectory);
            if (!string.IsNullOrEmpty(ffmpegRoot))
            {
                ffmpeg.RootPath = ffmpegRoot;
                ffmpeg.av_log_set_level(ffmpeg.AV_LOG_WARNING);
                librariesLoaded = true;
            }
        }

        /// <inheritdoc/>
        public bool IsPaused { get; private set; }

        /// <inheritdoc/>
        public event Action PlaybackFinished;

        /// <inheritdoc/>
        public void Play(string moviePath, bool mute)
        {
            Cleanup();
            playbackFinished = false;
            frameCount = 0;
            this.mute = mute;

            string relativeVideoPath = ContentPaths.GetVideoPath(moviePath);
            string fullPath = Path.Combine(
                ContentPaths.GetContentRootAbsolute(),
                ContentPaths.GetRelativePathWithContentFolder(relativeVideoPath)
            );

            if (!fileExists(fullPath) || !librariesLoaded)
            {
                PlaybackFinished?.Invoke();
                return;
            }

            if (!InitializeFfmpeg(fullPath))
            {
                Cleanup();
                PlaybackFinished?.Invoke();
                return;
            }

            waitForStart = true;
        }

        /// <inheritdoc/>
        public Texture2D GetTexture()
        {
            if (videoTexture == null || playbackFinished)
            {
                return null;
            }

            if (videoBuffer != null)
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

        /// <inheritdoc/>
        public bool IsPlaying()
        {
            return formatContext != null && !playbackFinished;
        }

        /// <inheritdoc/>
        public bool IsTextureReady()
        {
            return frameCount > 0 || (playbackStopwatch.IsRunning && playbackStopwatch.ElapsedMilliseconds > TextureReadyTimeoutMs);
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (playbackFinished)
            {
                return;
            }

            playbackFinished = true;
            playbackStopwatch.Stop();
            audioInstance?.Stop();
            Cleanup();
            PlaybackFinished?.Invoke();
        }

        /// <inheritdoc/>
        public void Pause()
        {
            if (!IsPaused)
            {
                IsPaused = true;
                playbackStopwatch.Stop();
                audioInstance?.Pause();
            }
        }

        /// <inheritdoc/>
        public void Resume()
        {
            if (IsPaused)
            {
                IsPaused = false;
                playbackStopwatch.Start();
                audioInstance?.Resume();
            }
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (!waitForStart)
            {
                return;
            }

            waitForStart = false;
            playbackStopwatch.Restart();
            if (!mute)
            {
                audioInstance?.Play();
            }
        }

        /// <inheritdoc/>
        public void Update()
        {
            if (waitForStart)
            {
                return;
            }

            if (IsPaused)
            {
                return;
            }

            if (!playbackFinished)
            {
                DecodeNextFrame();
                DrainAudioQueue();
            }

            if (playbackFinished && formatContext != null)
            {
                Cleanup();
                IsPaused = false;
                PlaybackFinished?.Invoke();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Cleanup();
        }

        /// <summary>
        /// Initializes FFmpeg contexts and opens the video file for decoding.
        /// </summary>
        /// <param name="filePath">Full path to the video file.</param>
        /// <returns><c>true</c> if initialization succeeded; otherwise, <c>false</c>.</returns>
        private bool InitializeFfmpeg(string filePath)
        {
            AVFormatContext* openedContext = null;
            if (ffmpeg.avformat_open_input(&openedContext, filePath, null, null) != 0)
            {
                return false;
            }

            formatContext = openedContext;

            if (ffmpeg.avformat_find_stream_info(formatContext, null) != 0)
            {
                return false;
            }

            videoStreamIndex = -1;
            for (uint i = 0; i < formatContext->nb_streams; i++)
            {
                AVStream* stream = formatContext->streams[i];
                if (stream->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    videoStreamIndex = (int)i;
                    break;
                }
            }

            if (videoStreamIndex < 0)
            {
                return false;
            }

            AVStream* videoStream = formatContext->streams[videoStreamIndex];
            AVCodec* codec = ffmpeg.avcodec_find_decoder(videoStream->codecpar->codec_id);
            if (codec == null)
            {
                return false;
            }

            videoCodecContext = ffmpeg.avcodec_alloc_context3(codec);
            if (videoCodecContext == null)
            {
                return false;
            }

            if (ffmpeg.avcodec_parameters_to_context(videoCodecContext, videoStream->codecpar) < 0)
            {
                return false;
            }

            if (ffmpeg.avcodec_open2(videoCodecContext, codec, null) < 0)
            {
                return false;
            }

            videoWidth = videoCodecContext->width;
            videoHeight = videoCodecContext->height;
            if (videoWidth <= 0 || videoHeight <= 0)
            {
                return false;
            }

            videoFrame = ffmpeg.av_frame_alloc();
            rgbaFrame = ffmpeg.av_frame_alloc();
            if (videoFrame == null || rgbaFrame == null)
            {
                return false;
            }

            int rgbaBufferSize = checked(videoWidth * videoHeight * 4);
            rgbaBuffer = (byte*)ffmpeg.av_malloc((ulong)rgbaBufferSize);
            if (rgbaBuffer == null)
            {
                return false;
            }

            rgbaFrame->format = (int)AVPixelFormat.AV_PIX_FMT_RGBA;
            rgbaFrame->width = videoWidth;
            rgbaFrame->height = videoHeight;
            rgbaFrame->data[0] = rgbaBuffer;
            rgbaFrame->linesize[0] = videoWidth * 4;

            swsContext = ffmpeg.sws_getContext(
                videoWidth,
                videoHeight,
                videoCodecContext->pix_fmt,
                videoWidth,
                videoHeight,
                AVPixelFormat.AV_PIX_FMT_RGBA,
                (int)SwsFlags.SWS_BILINEAR,
                null,
                null,
                null);

            AVRational timeBase = videoStream->time_base;
            videoTimeBase = timeBase.num / (double)timeBase.den;
            nextFramePts = 0;

            if (swsContext == null)
            {
                return false;
            }

            packet = ffmpeg.av_packet_alloc();
            if (packet == null)
            {
                return false;
            }

            if (!mute && !InitializeAudio())
            {
                CleanupAudio();
                Console.WriteLine("[FFmpeg] Audio init failed; continuing without audio.");
            }

            return true;
        }

        /// <summary>
        /// Decodes the next video frame and updates the frame buffer.
        /// </summary>
        /// <remarks>
        /// Uses presentation timestamps for frame timing synchronization.
        /// Also processes audio packets encountered during decoding.
        /// </remarks>
        private void DecodeNextFrame()
        {
            if (formatContext == null || packet == null || videoCodecContext == null)
            {
                playbackFinished = true;
                return;
            }

            if (!playbackStopwatch.IsRunning)
            {
                return;
            }

            double elapsedSeconds = GetPlaybackClock();
            if (elapsedSeconds < nextFramePts)
            {
                return;
            }

            while (true)
            {
                int readResult = ffmpeg.av_read_frame(formatContext, packet);
                if (readResult < 0)
                {
                    playbackFinished = true;
                    return;
                }

                if (packet->stream_index == audioStreamIndex && !mute && audioCodecContext != null)
                {
                    DecodeAudioPacket(packet);
                    ffmpeg.av_packet_unref(packet);
                    continue;
                }

                if (packet->stream_index != videoStreamIndex)
                {
                    ffmpeg.av_packet_unref(packet);
                    continue;
                }

                int sendResult = ffmpeg.avcodec_send_packet(videoCodecContext, packet);
                ffmpeg.av_packet_unref(packet);
                if (sendResult < 0)
                {
                    playbackFinished = true;
                    return;
                }

                int receiveResult = ffmpeg.avcodec_receive_frame(videoCodecContext, videoFrame);
                if (receiveResult == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                {
                    continue;
                }

                if (receiveResult == ffmpeg.AVERROR_EOF)
                {
                    playbackFinished = true;
                    return;
                }

                if (receiveResult < 0)
                {
                    playbackFinished = true;
                    return;
                }

                long pts = videoFrame->best_effort_timestamp;
                if (pts != ffmpeg.AV_NOPTS_VALUE)
                {
                    nextFramePts = pts * videoTimeBase;
                }

                _ = ffmpeg.sws_scale(
                    swsContext,
                    videoFrame->data,
                    videoFrame->linesize,
                    0,
                    videoHeight,
                    rgbaFrame->data,
                    rgbaFrame->linesize);

                EnsureTexture(videoWidth, videoHeight);
                EnsureBuffer(videoWidth, videoHeight);

                int srcStride = rgbaFrame->linesize[0];
                int dstStride = videoWidth * 4;
                byte* srcBase = rgbaFrame->data[0];
                if (srcBase == null)
                {
                    playbackFinished = true;
                    return;
                }

                fixed (byte* dstBase = videoBuffer)
                {
                    for (int y = 0; y < videoHeight; y++)
                    {
                        byte* srcRow = srcBase + (y * srcStride);
                        byte* dstRow = dstBase + (y * dstStride);
                        Buffer.MemoryCopy(srcRow, dstRow, dstStride, dstStride);
                    }
                }

                lock (bufferLock)
                {
                    frameReady = true;
                }

                frameCount++;
                return;
            }
        }

        /// <summary>
        /// Initializes audio decoding and playback components.
        /// </summary>
        /// <returns><c>true</c> if audio initialization succeeded or no audio stream exists; otherwise, <c>false</c>.</returns>
        private bool InitializeAudio()
        {
            audioStreamIndex = -1;
            for (uint i = 0; i < formatContext->nb_streams; i++)
            {
                AVStream* stream = formatContext->streams[i];
                if (stream->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    audioStreamIndex = (int)i;
                    break;
                }
            }

            if (audioStreamIndex < 0)
            {
                return true;
            }

            AVStream* audioStream = formatContext->streams[audioStreamIndex];
            AVCodec* audioCodec = ffmpeg.avcodec_find_decoder(audioStream->codecpar->codec_id);
            if (audioCodec == null)
            {
                return false;
            }

            audioCodecContext = ffmpeg.avcodec_alloc_context3(audioCodec);
            if (audioCodecContext == null)
            {
                return false;
            }

            if (ffmpeg.avcodec_parameters_to_context(audioCodecContext, audioStream->codecpar) < 0)
            {
                return false;
            }

            if (ffmpeg.avcodec_open2(audioCodecContext, audioCodec, null) < 0)
            {
                return false;
            }

            audioSampleRate = audioCodecContext->sample_rate;
            int inputChannels = audioCodecContext->ch_layout.nb_channels;
            if (inputChannels <= 0)
            {
                inputChannels = 2;
            }

            audioChannels = inputChannels <= 1 ? 1 : 2;

            AVChannelLayout inLayout = audioCodecContext->ch_layout;
            AVChannelLayout outLayout = default;
            ffmpeg.av_channel_layout_default(&outLayout, audioChannels);

            SwrContext* swr = null;
            int swrResult = ffmpeg.swr_alloc_set_opts2(
                &swr,
                &outLayout,
                AVSampleFormat.AV_SAMPLE_FMT_S16,
                audioSampleRate,
                &inLayout,
                audioCodecContext->sample_fmt,
                audioCodecContext->sample_rate,
                0,
                null);

            ffmpeg.av_channel_layout_uninit(&outLayout);

            if (swrResult < 0 || swr == null)
            {
                return false;
            }

            swrContext = swr;

            if (ffmpeg.swr_init(swrContext) < 0)
            {
                return false;
            }

            audioFrame = ffmpeg.av_frame_alloc();
            if (audioFrame == null)
            {
                return false;
            }

            AudioChannels channels = audioChannels == 1 ? AudioChannels.Mono : AudioChannels.Stereo;
            audioInstance = new DynamicSoundEffectInstance(audioSampleRate, channels);

            return true;
        }

        /// <summary>
        /// Decodes an audio packet and queues the samples for playback.
        /// </summary>
        /// <param name="audioPacket">The FFmpeg audio packet to decode.</param>
        private void DecodeAudioPacket(AVPacket* audioPacket)
        {
            if (audioCodecContext == null || audioFrame == null || swrContext == null || audioInstance == null)
            {
                return;
            }

            int sendResult = ffmpeg.avcodec_send_packet(audioCodecContext, audioPacket);
            if (sendResult < 0)
            {
                return;
            }

            byte** outBuffers = stackalloc byte*[1];
            while (true)
            {
                int receiveResult = ffmpeg.avcodec_receive_frame(audioCodecContext, audioFrame);
                if (receiveResult == ffmpeg.AVERROR(ffmpeg.EAGAIN) || receiveResult == ffmpeg.AVERROR_EOF)
                {
                    return;
                }

                if (receiveResult < 0)
                {
                    playbackFinished = true;
                    return;
                }

                long delay = ffmpeg.swr_get_delay(swrContext, audioCodecContext->sample_rate);
                int dstSampleCount = (int)ffmpeg.av_rescale_rnd(
                    delay + audioFrame->nb_samples,
                    audioSampleRate,
                    audioCodecContext->sample_rate,
                    AVRounding.AV_ROUND_UP);

                int requiredBufferSize = ffmpeg.av_samples_get_buffer_size(
                    null,
                    audioChannels,
                    dstSampleCount,
                    AVSampleFormat.AV_SAMPLE_FMT_S16,
                    1);

                if (requiredBufferSize <= 0)
                {
                    continue;
                }

                EnsureAudioBuffer(requiredBufferSize);

                outBuffers[0] = audioBuffer;

                int convertedSamples = ffmpeg.swr_convert(
                    swrContext,
                    outBuffers,
                    dstSampleCount,
                    audioFrame->extended_data,
                    audioFrame->nb_samples);

                if (convertedSamples <= 0)
                {
                    continue;
                }

                int convertedSize = ffmpeg.av_samples_get_buffer_size(
                    null,
                    audioChannels,
                    convertedSamples,
                    AVSampleFormat.AV_SAMPLE_FMT_S16,
                    1);

                if (convertedSize <= 0)
                {
                    continue;
                }

                SubmitAudioBuffer(convertedSize);
            }
        }

        /// <summary>
        /// Ensures the audio buffer has sufficient capacity.
        /// </summary>
        /// <param name="requiredSize">The minimum required buffer size in bytes.</param>
        private void EnsureAudioBuffer(int requiredSize)
        {
            if (audioBuffer != null && audioBufferCapacity >= requiredSize)
            {
                return;
            }

            if (audioBuffer != null)
            {
                ffmpeg.av_free(audioBuffer);
            }

            audioBuffer = (byte*)ffmpeg.av_malloc((ulong)requiredSize);
            audioBufferCapacity = requiredSize;
        }

        /// <summary>
        /// Copies audio data to managed memory and queues it for playback.
        /// </summary>
        /// <param name="size">The size of audio data in bytes.</param>
        private void SubmitAudioBuffer(int size)
        {
            if (audioInstance == null || audioBuffer == null)
            {
                return;
            }

            byte[] managedBuffer = new byte[size];
            Marshal.Copy((IntPtr)audioBuffer, managedBuffer, 0, size);
            pendingAudioQueue.Enqueue(managedBuffer);

            DrainAudioQueue();
        }

        /// <summary>
        /// Submits queued audio buffers to the sound effect instance.
        /// </summary>
        private void DrainAudioQueue()
        {
            if (audioInstance == null)
            {
                return;
            }

            while (pendingAudioQueue.Count > 0 && audioInstance.PendingBufferCount < MaxQueuedAudioBuffers)
            {
                byte[] buffer = pendingAudioQueue.Dequeue();
                audioInstance.SubmitBuffer(buffer, 0, buffer.Length);
                audioBytesDrained += buffer.Length;
                audioBuffersSubmitted++;
            }
        }

        /// <summary>
        /// Gets the current playback position in seconds.
        /// </summary>
        /// <returns>The elapsed playback time in seconds.</returns>
        private double GetPlaybackClock()
        {
            // Use stopwatch as primary clock - it pauses correctly and resumes properly
            // Audio sync is handled by buffering; the stopwatch provides consistent timing
            return playbackStopwatch.Elapsed.TotalSeconds;
        }

        /// <summary>
        /// Ensures the video texture exists and matches the specified dimensions.
        /// </summary>
        /// <param name="width">Required texture width.</param>
        /// <param name="height">Required texture height.</param>
        private void EnsureTexture(int width, int height)
        {
            if (videoTexture != null && width == textureWidth && height == textureHeight)
            {
                return;
            }

            videoTexture?.Dispose();
            videoTexture = new Texture2D(Global.GraphicsDevice, width, height, false, SurfaceFormat.Color);
            textureWidth = width;
            textureHeight = height;
        }

        /// <summary>
        /// Ensures the video buffer array has sufficient capacity for the frame data.
        /// </summary>
        /// <param name="width">Frame width in pixels.</param>
        /// <param name="height">Frame height in pixels.</param>
        private void EnsureBuffer(int width, int height)
        {
            int bufferSize = checked(width * height * 4);
            if (videoBuffer == null || videoBuffer.Length != bufferSize)
            {
                videoBuffer = new byte[bufferSize];
            }
        }

        /// <summary>
        /// Releases all FFmpeg and video resources.
        /// </summary>
        private void Cleanup()
        {
            if (packet != null)
            {
                AVPacket* packetToFree = packet;
                ffmpeg.av_packet_free(&packetToFree);
                packet = null;
            }

            if (swsContext != null)
            {
                ffmpeg.sws_freeContext(swsContext);
                swsContext = null;
            }

            if (videoFrame != null)
            {
                AVFrame* frameToFree = videoFrame;
                ffmpeg.av_frame_free(&frameToFree);
                videoFrame = null;
            }

            if (rgbaFrame != null)
            {
                AVFrame* frameToFree = rgbaFrame;
                ffmpeg.av_frame_free(&frameToFree);
                rgbaFrame = null;
            }

            if (videoCodecContext != null)
            {
                AVCodecContext* contextToFree = videoCodecContext;
                ffmpeg.avcodec_free_context(&contextToFree);
                videoCodecContext = null;
            }

            if (formatContext != null)
            {
                AVFormatContext* contextToClose = formatContext;
                ffmpeg.avformat_close_input(&contextToClose);
                formatContext = null;
            }

            if (rgbaBuffer != null)
            {
                ffmpeg.av_free(rgbaBuffer);
                rgbaBuffer = null;
            }

            CleanupAudio();

            videoTexture?.Dispose();
            videoTexture = null;
            videoBuffer = null;
            frameReady = false;
            waitForStart = false;
            playbackStopwatch.Reset();
            videoStreamIndex = -1;
            videoWidth = 0;
            videoHeight = 0;
            textureWidth = 0;
            textureHeight = 0;
            frameCount = 0;
            videoTimeBase = 0;
            nextFramePts = 0;
        }

        /// <summary>
        /// Releases all audio-related resources.
        /// </summary>
        private void CleanupAudio()
        {
            pendingAudioQueue.Clear();
            audioBytesDrained = 0;
            audioBuffersSubmitted = 0;

            if (audioInstance != null)
            {
                audioInstance.Stop();
                audioInstance.Dispose();
                audioInstance = null;
            }

            if (audioFrame != null)
            {
                AVFrame* frameToFree = audioFrame;
                ffmpeg.av_frame_free(&frameToFree);
                audioFrame = null;
            }

            if (swrContext != null)
            {
                SwrContext* swrToFree = swrContext;
                ffmpeg.swr_free(&swrToFree);
                swrContext = null;
            }

            if (audioCodecContext != null)
            {
                AVCodecContext* contextToFree = audioCodecContext;
                ffmpeg.avcodec_free_context(&contextToFree);
                audioCodecContext = null;
            }

            if (audioBuffer != null)
            {
                ffmpeg.av_free(audioBuffer);
                audioBuffer = null;
                audioBufferCapacity = 0;
            }

            audioStreamIndex = -1;
            audioChannels = 0;
            audioSampleRate = 0;
        }

        /// <summary>FFmpeg format/demuxer context for the video file.</summary>
        private AVFormatContext* formatContext;

        /// <summary>Video decoder context.</summary>
        private AVCodecContext* videoCodecContext;

        /// <summary>Decoded video frame in native pixel format.</summary>
        private AVFrame* videoFrame;

        /// <summary>Video frame converted to RGBA format.</summary>
        private AVFrame* rgbaFrame;

        /// <summary>Pixel format conversion context.</summary>
        private SwsContext* swsContext;

        /// <summary>Reusable packet for reading compressed data.</summary>
        private AVPacket* packet;

        /// <summary>Native buffer for RGBA frame data.</summary>
        private byte* rgbaBuffer;

        /// <summary>Index of the video stream in the container.</summary>
        private int videoStreamIndex;

        /// <summary>Video width in pixels.</summary>
        private int videoWidth;

        /// <summary>Video height in pixels.</summary>
        private int videoHeight;

        /// <summary>Current texture width.</summary>
        private int textureWidth;

        /// <summary>Current texture height.</summary>
        private int textureHeight;

        /// <summary>Number of frames decoded so far.</summary>
        private int frameCount;

        /// <summary>Indicates a new frame is ready to be uploaded to the texture.</summary>
        private bool frameReady;

        /// <summary>Indicates playback has finished.</summary>
        private bool playbackFinished;

        /// <summary>Indicates the player is waiting for Start() to be called.</summary>
        private bool waitForStart;

        /// <summary>Indicates audio should be muted.</summary>
        private bool mute;

        /// <summary>Time base for converting video timestamps to seconds.</summary>
        private double videoTimeBase;

        /// <summary>Presentation timestamp of the next frame to display.</summary>
        private double nextFramePts;

        /// <summary>MonoGame texture for rendering video frames.</summary>
        private Texture2D videoTexture;

        /// <summary>Managed buffer for transferring frame data to the texture.</summary>
        private byte[] videoBuffer;

        /// <summary>Audio decoder context.</summary>
        private AVCodecContext* audioCodecContext;

        /// <summary>Decoded audio frame.</summary>
        private AVFrame* audioFrame;

        /// <summary>Audio resampling context.</summary>
        private SwrContext* swrContext;

        /// <summary>Index of the audio stream in the container.</summary>
        private int audioStreamIndex;

        /// <summary>Number of audio channels (1 for mono, 2 for stereo).</summary>
        private int audioChannels;

        /// <summary>Audio sample rate in Hz.</summary>
        private int audioSampleRate;

        /// <summary>MonoGame dynamic sound effect for audio playback.</summary>
        private DynamicSoundEffectInstance audioInstance;

        /// <summary>Native buffer for resampled audio data.</summary>
        private byte* audioBuffer;

        /// <summary>Current capacity of the audio buffer.</summary>
        private int audioBufferCapacity;

        /// <summary>Total bytes of audio data drained to the sound instance.</summary>
        private long audioBytesDrained;

        /// <summary>Number of audio buffers submitted to the sound instance.</summary>
        private int audioBuffersSubmitted;
    }
}
#endif
