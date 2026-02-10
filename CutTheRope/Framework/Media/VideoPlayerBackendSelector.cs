namespace CutTheRope.Framework.Media
{
    /// <summary>
    /// Available video player backend implementations.
    /// </summary>
    internal enum VideoPlayerBackend
    {
        /// <summary>macOS AVFoundation framework (requires macOS 26+).</summary>
        AVFoundation,

        /// <summary>FFmpeg libraries for cross-platform video decoding.</summary>
        Ffmpeg,

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
        /// <returns>The selected <see cref="VideoPlayerBackend"/> to use.</returns>
        /// <remarks>
        /// Selection priority on macOS 26+: AVFoundation → FFmpeg → MonoGame stub.
        /// Selection priority on other platforms: FFmpeg → MonoGame stub.
        /// </remarks>
        public static VideoPlayerBackend Select(bool isMac, bool isMac26OrLater, bool hasAvFoundation, bool hasFfmpeg)
        {
            return isMac && isMac26OrLater && hasAvFoundation
                ? VideoPlayerBackend.AVFoundation
                : hasFfmpeg ? VideoPlayerBackend.Ffmpeg : VideoPlayerBackend.MonoGame;
        }
    }
}
