#if MACOS_FFMPEG
using System;
using System.IO;
using System.Runtime.InteropServices;

using CutTheRope.Desktop;
using CutTheRope.Helpers;

using FFmpeg.AutoGen;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace CutTheRope.Framework.Media
{
    internal sealed unsafe class VideoPlayerFFmpeg : IVideoPlayer
    {
        private const int TextureReadyTimeoutMs = 500;
        private const int MaxQueuedAudioBuffers = 4;
        private readonly object bufferLock = new();
        private readonly Func<string, bool> fileExists;
        private readonly Func<string, string> resolveRootPath;

        public VideoPlayerFFmpeg()
            : this(File.Exists, baseDir => FfmpegRootPathResolver.Resolve(baseDir, Directory.Exists, File.Exists))
        {
        }

        internal VideoPlayerFFmpeg(Func<string, bool> fileExists, Func<string, string> resolveRootPath)
        {
            this.fileExists = fileExists;
            this.resolveRootPath = resolveRootPath;
        }

        public bool IsPaused { get; private set; }

        public event Action PlaybackFinished;

        public void Play(string moviePath, bool mute)
        {
            Cleanup();
            playbackFinished = false;
            frameCount = 0;
            playStartTime = null;
            this.mute = mute;

            string relativeVideoPath = ContentPaths.GetVideoPath(moviePath);
            string fullPath = Path.Combine(
                AppContext.BaseDirectory,
                ContentPaths.RootDirectory,
                ContentPaths.GetRelativePathWithContentFolder(relativeVideoPath)
            );

            if (!fileExists(fullPath))
            {
                PlaybackFinished?.Invoke();
                return;
            }

            string ffmpegRoot = resolveRootPath(AppContext.BaseDirectory);
            if (string.IsNullOrEmpty(ffmpegRoot))
            {
                PlaybackFinished?.Invoke();
                return;
            }

            ffmpeg.RootPath = ffmpegRoot;
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_WARNING);

            if (!InitializeFfmpeg(fullPath))
            {
                Cleanup();
                PlaybackFinished?.Invoke();
                return;
            }

            waitForStart = true;
        }

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

        public bool IsPlaying()
        {
            return formatContext != null && !playbackFinished;
        }

        public bool IsTextureReady()
        {
            if (frameCount > 0)
            {
                return true;
            }

            return playStartTime.HasValue && (DateTime.UtcNow - playStartTime.Value).TotalMilliseconds > TextureReadyTimeoutMs;
        }

        public void Stop()
        {
            if (playbackFinished)
            {
                return;
            }

            playbackFinished = true;
            audioInstance?.Stop();
        }

        public void Pause()
        {
            if (!IsPaused)
            {
                IsPaused = true;
                audioInstance?.Pause();
            }
        }

        public void Resume()
        {
            if (IsPaused)
            {
                IsPaused = false;
                audioInstance?.Resume();
            }
        }

        public void Start()
        {
            if (!waitForStart)
            {
                return;
            }

            waitForStart = false;
            playStartTime = DateTime.UtcNow;
            if (!mute)
            {
                audioInstance?.Play();
            }
        }

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
            }

            if (playbackFinished)
            {
                Cleanup();
                IsPaused = false;
                PlaybackFinished?.Invoke();
            }
        }

        public void Dispose()
        {
            Cleanup();
        }

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

        private void DecodeNextFrame()
        {
            if (formatContext == null || packet == null || videoCodecContext == null)
            {
                playbackFinished = true;
                return;
            }

            if (!playStartTime.HasValue)
            {
                return;
            }

            double elapsedSeconds = (DateTime.UtcNow - playStartTime.Value).TotalSeconds;
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

                ffmpeg.sws_scale(
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

        private void SubmitAudioBuffer(int size)
        {
            if (audioInstance == null || audioBuffer == null)
            {
                return;
            }

            if (audioInstance.PendingBufferCount >= MaxQueuedAudioBuffers)
            {
                return;
            }

            byte[] managedBuffer = new byte[size];
            Marshal.Copy((IntPtr)audioBuffer, managedBuffer, 0, size);
            audioInstance.SubmitBuffer(managedBuffer, 0, size);

            if (audioInstance.State == SoundState.Stopped)
            {
                audioInstance.Play();
            }
        }

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

        private void EnsureBuffer(int width, int height)
        {
            int bufferSize = checked(width * height * 4);
            if (videoBuffer == null || videoBuffer.Length != bufferSize)
            {
                videoBuffer = new byte[bufferSize];
            }
        }

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
            playbackFinished = false;
            waitForStart = false;
            playStartTime = null;
            videoStreamIndex = -1;
            videoWidth = 0;
            videoHeight = 0;
            textureWidth = 0;
            textureHeight = 0;
            frameCount = 0;
            videoTimeBase = 0;
            nextFramePts = 0;
        }

        private void CleanupAudio()
        {
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

        private AVFormatContext* formatContext;
        private AVCodecContext* videoCodecContext;
        private AVFrame* videoFrame;
        private AVFrame* rgbaFrame;
        private SwsContext* swsContext;
        private AVPacket* packet;
        private byte* rgbaBuffer;
        private int videoStreamIndex;
        private int videoWidth;
        private int videoHeight;
        private int textureWidth;
        private int textureHeight;
        private int frameCount;
        private bool frameReady;
        private bool playbackFinished;
        private bool waitForStart;
        private bool mute;
        private DateTime? playStartTime;
        private double videoTimeBase;
        private double nextFramePts;
        private Texture2D videoTexture;
        private byte[] videoBuffer;
        private AVCodecContext* audioCodecContext;
        private AVFrame* audioFrame;
        private SwrContext* swrContext;
        private int audioStreamIndex;
        private int audioChannels;
        private int audioSampleRate;
        private DynamicSoundEffectInstance audioInstance;
        private byte* audioBuffer;
        private int audioBufferCapacity;
    }
}
#endif
