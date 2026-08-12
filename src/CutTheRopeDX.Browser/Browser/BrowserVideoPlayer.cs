using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Framework.Platform;

namespace CutTheRopeDX.Browser
{
    /// <summary>Cutscene playback, backed by a video element laid over the canvas.</summary>
    /// <remarks>
    /// The desktop backends decode frames into a texture Core then draws. A browser has no
    /// reason to: the element composites itself over the canvas and keeps its own audio in
    /// sync, so <see cref="GetTexture"/> has nothing to hand back and the host's
    /// <c>DrawMovie</c> only has to leave the surface black underneath.
    /// </remarks>
    internal sealed partial class BrowserVideoPlayer : IVideoPlayer
    {
        /// <summary>Directory the web content pipeline writes cutscenes to.</summary>
        private const string VideoDirectory = "./content/video_hd/";

        /// <summary>Extension the web content pipeline writes cutscenes as.</summary>
        private const string VideoExtension = ".webm";

        private bool _loaded;
        private bool _started;

        /// <summary>Imports video.js. Must be awaited once before any other call.</summary>
        public static Task ImportAsync()
        {
            return JSHost.ImportAsync("video", "../video.js");
        }

        /// <inheritdoc />
        public bool IsPaused { get; private set; }

        /// <inheritdoc />
        public event Action PlaybackFinished;

        /// <inheritdoc />
        public void Play(string moviePath, bool mute)
        {
            _loaded = true;
            _started = false;
            IsPaused = false;
            Load(VideoDirectory + moviePath + VideoExtension, mute);
        }

        /// <inheritdoc />
        public void Start()
        {
            // Core calls this every frame the movie view is up. The module ignores repeat
            // calls anyway; the flag keeps it from crossing the interop boundary 60 times
            // a second to be told so.
            if (!_loaded || _started)
            {
                return;
            }
            _started = true;
            StartPlayback();
        }

        /// <inheritdoc />
        public void Update()
        {
            if (!_loaded || !IsFinished())
            {
                return;
            }

            _loaded = false;
            _started = false;
            IsPaused = false;
            PlaybackFinished?.Invoke();
        }

        /// <inheritdoc />
        public void Stop()
        {
            StopPlayback();
        }

        /// <inheritdoc />
        public void Pause()
        {
            if (!_loaded || IsPaused)
            {
                return;
            }
            IsPaused = true;
            PausePlayback();
        }

        /// <inheritdoc />
        public void Resume()
        {
            if (!_loaded || !IsPaused)
            {
                return;
            }
            IsPaused = false;
            ResumePlayback();
        }

        /// <inheritdoc />
        public bool IsPlaying()
        {
            return _loaded;
        }

        /// <inheritdoc />
        public bool IsTextureReady()
        {
            return true;
        }

        /// <inheritdoc />
        public ITextureHandle GetTexture()
        {
            return null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            StopPlayback();
        }

        [JSImport("load", "video")]
        private static partial void Load(string url, bool mute);

        [JSImport("start", "video")]
        private static partial void StartPlayback();

        [JSImport("pause", "video")]
        private static partial void PausePlayback();

        [JSImport("resume", "video")]
        private static partial void ResumePlayback();

        [JSImport("stop", "video")]
        private static partial void StopPlayback();

        [JSImport("isFinished", "video")]
        private static partial bool IsFinished();
    }
}
