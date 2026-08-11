using System;

using CutTheRopeDX.Framework.Core;
using CutTheRopeDX.Framework.Media;
using CutTheRopeDX.Helpers;

namespace CutTheRopeDX.Framework.Platform
{
    /// <summary>
    /// The platform seam registry. The desktop host populates these at boot;
    /// headless (and later web) installs only what exists there. Optional
    /// services stay null and call sites use "?.".
    /// </summary>
    internal static class PlatformServices
    {
        public static IRichPresence RichPresence { get; set; }
        public static IUpdateService Updates { get; set; }
        public static ICursorService Cursor { get; set; }
        public static IHostApp Host { get; set; }
        public static IWindowService Window { get; set; }
        public static IRenderBackend Render { get; set; }
        public static IFileWatcherFactory FileWatchers { get; set; }

        /// <summary>
        /// Raw content byte access. Defaults to the deployed content directory; the browser
        /// host replaces it with a fetch-backed store during boot.
        /// </summary>
        public static IContentStore Content { get; set; }
            = new FileContentStore(ContentPaths.GetContentRootAbsolute());

        /// <summary>
        /// Preference persistence. The desktop host installs a file-backed store at boot;
        /// the browser host installs a localStorage-backed one.
        /// </summary>
        public static IPreferenceStore Preferences { get; set; }

        /// <summary>
        /// Creates the video player backend to use for the current platform build. The choice
        /// depends on compile-time constants (<c>MACOS_AVFOUNDATION</c>, <c>FFMPEG_BACKEND</c>) that
        /// only the desktop host's build defines, so the desktop host installs this at boot; when
        /// absent (headless), <see cref="VideoPlayerMonoGame"/> — a Core-owned no-op stub — is used.
        /// </summary>
        public static Func<IVideoPlayer> VideoPlayerFactory { get; set; }
    }
}
