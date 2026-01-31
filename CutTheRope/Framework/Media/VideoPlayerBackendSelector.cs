namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Available video player backend implementations.
    /// </summary>
    internal enum VideoPlayerBackend
    {
        /// <summary>macOS AVFoundation framework (requires macOS 26+).</summary>
        AVFoundation,

        /// <summary>FFmpeg libraries for cross-platform decoding (used for macOS below 26).</summary>
        Ffmpeg,

        /// <summary>VLC media player library.</summary>
        Vlc,

        /// <summary>Stub implementation that skips video playback.</summary>
        MonoGame
    }

    /// <summary>
    /// Selects the appropriate video player backend based on platform and available libraries.
    /// </summary>
    internal static class VideoPlayerBackendSelector
    {
        /// <summary>
        /// Determines the best available video player backend.
        /// </summary>
        /// <param name="isMac">Whether the current platform is macOS.</param>
        /// <param name="isMac26OrLater">Whether the macOS version is 26 or later.</param>
        /// <param name="hasAvFoundation">Whether AVFoundation support is compiled in.</param>
        /// <param name="hasFfmpeg">Whether FFmpeg support is compiled in.</param>
        /// <param name="hasVlc">Whether VLC support is compiled in.</param>
        /// <returns>The selected <see cref="VideoPlayerBackend"/> to use.</returns>
        /// <remarks>
        /// Selection priority on macOS: AVFoundation (if macOS 26+) → FFmpeg → VLC → MonoGame stub.
        /// Selection priority on other platforms: VLC → MonoGame stub.
        /// </remarks>
        public static VideoPlayerBackend Select(bool isMac, bool isMac26OrLater, bool hasAvFoundation, bool hasFfmpeg, bool hasVlc)
        {
            if (isMac)
            {
                if (isMac26OrLater && hasAvFoundation)
                {
                    return VideoPlayerBackend.AVFoundation;
                }

                if (hasFfmpeg)
                {
                    return VideoPlayerBackend.Ffmpeg;
                }
            }

            return hasVlc ? VideoPlayerBackend.Vlc : VideoPlayerBackend.MonoGame;
        }
    }
}
